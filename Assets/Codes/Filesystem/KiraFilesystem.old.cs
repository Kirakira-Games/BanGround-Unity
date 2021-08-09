/*using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace System.IO
{
    [Obsolete]
    class KiraFilesystem : IDisposable
    {
        public static KiraFilesystem Instance;

        // file, kirapack
        Dictionary<string, string> index;

        Dictionary<string, ZipArchive> openedArchive = new Dictionary<string, ZipArchive>();
        Dictionary<string, DateTime> lastAccessTime = new Dictionary<string, DateTime>();

        Thread thread;
        bool disposed = false;

        string indexFile;
        string root;
        string tempPath;

        public KiraFilesystem(string indexFile, string filesystemRoot)
        {
            if (Instance == null)
                Instance = this;

            this.indexFile = indexFile;
            root = filesystemRoot;
            tempPath = $"{root}temp/";

            DirectoryInfo di = new DirectoryInfo(tempPath);
            if (!di.Exists) di.Create();

            if (File.Exists(indexFile))
            {
                using (FileStream fs = File.OpenRead(indexFile))
                {
                    var bf = new BinaryFormatter();
                    index = bf.Deserialize(fs) as Dictionary<string, string>;
                }
            }
            else
            {
                index = new Dictionary<string, string>();
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var count = index.Update(value => true, value => root + value.Substring(92));

                if (count > 0)
                    SaveIndex();
            }

            thread = new Thread(() =>
            {
                while(true)
                {
                    if (disposed)
                        break;

                    ReleaseUnusedKirapacks();

                    Thread.Sleep(1000);
                }
            });

            thread.Start();
        }

        public void AddToIndex(string kiraPack)
        {
            using (var s = File.OpenRead(kiraPack))
            {
                using (var zip = new ZipArchive(s))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (!entry.FullName.EndsWith("/") && entry.Length != 0)
                        {
                            if (Exists(entry.FullName))
                                RemoveFileFromIndex(entry.FullName);

                            var path = KiraPath.Combine(root, entry.FullName);
                            if (File.Exists(path))
                                File.Delete(path);

                            index.Add(entry.FullName, kiraPack);
                        }
                    }
                }
            }

            index.Add(kiraPack, kiraPack);
        }

        public void RemoveFromIndex(string kiraPack)
        {
            if (openedArchive.ContainsKey(kiraPack))
            {
                openedArchive[kiraPack].Dispose();

                openedArchive.Remove(kiraPack);
                lastAccessTime.Remove(kiraPack);
            }

            var entries = (from x in index where x.Value == kiraPack select x.Key).ToArray();

            foreach (var entry in entries)
                index.Remove(entry);

            index.Remove(kiraPack);
        }

        public void RemoveFileFromIndex(string fileName)
        {
            var path = KiraPath.Combine(root, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (index.ContainsKey(fileName))
            {
                index.Remove(fileName);
            }
        }

        public void CleanUnusedKirapack()
        {
            var query = from x in index
                        where x.Key.Contains("data/filesystem/") && (from y in index where y.Value == x.Key select y).Count() == 1
                        select x.Key;

            query.Any(kirapack =>
            {
                index.Remove(kirapack);
                if(openedArchive.ContainsKey(kirapack))
                {
                    openedArchive[kirapack].Dispose();
                    openedArchive.Remove(kirapack);
                    lastAccessTime.Remove(kirapack);
                }

                File.Delete(kirapack);

                return true;
            });
        }

        public void SaveIndex()
        {
            if (index.ContainsKey("chart/"))
                index.Remove("chart/");
            if (index.ContainsKey("music/"))
                index.Remove("music/");

            CleanUnusedKirapack();

            if (File.Exists(indexFile))
                File.Delete(indexFile);

            using (FileStream fs = File.OpenWrite(indexFile))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, index);
            }
        }

        public string[] IndexedFiles
        {
            get
            {
                string[] files = new string[index.Count];
                int i = 0;
                foreach (var kv in index)
                    files[i++] = kv.Key;

                return files;
            }
        }

        private void GetFiles(List<string> files, DirectoryInfo di, Func<string, bool> func)
        {
            var subfiles = from x in di.GetFiles()
                           where func(x.FullName.Replace('\\', '/').Replace(root, ""))
                           select x.FullName.Replace('\\','/').Replace(root, "");

            var subdirs = di.GetDirectories();

            files.AddRange(subfiles);

            foreach (var sdi in subdirs)
                GetFiles(files, sdi, func);
        }

        public string[] ListFiles(Func<string, bool> func = null)
        {
            if (func == null)
                func = name => true;

            var files = (from x in index where func(x.Key) select x.Key).ToList();
            GetFiles(files, new DirectoryInfo(root), func);
            return files.ToArray();
        }

        public bool Exists(string fileName)
        {
            if (File.Exists(KiraPath.Combine(root, fileName)))
                return true;

            if (!index.ContainsKey(fileName))
                return false;

            var targetKirapack = index[fileName];

            if (!File.Exists(targetKirapack))
                return false;

            ZipArchive zip = null;

            if (openedArchive.ContainsKey(targetKirapack))
            {
                zip = openedArchive[targetKirapack];
            }
            else
            {
                zip = ZipFile.OpenRead(targetKirapack);
                openedArchive.Add(targetKirapack, zip);
            }

            lastAccessTime[targetKirapack] = DateTime.Now;

            if (zip.GetEntry(fileName) != null)
                return true;

            return false;
        }

        public byte[] Read(string fileName)
        {
            if (File.Exists(KiraPath.Combine(root, fileName)))
            {
                return File.ReadAllBytes(KiraPath.Combine(root, fileName));
            }

            if (!index.ContainsKey(fileName))
                throw new FileNotFoundException($"File {fileName} not found in filesystem.");

            var targetKirapack = index[fileName];

            ZipArchive zip = null;

            if (openedArchive.ContainsKey(targetKirapack))
            {
                zip = openedArchive[targetKirapack];
            }
            else
            {
                zip = ZipFile.OpenRead(targetKirapack);
                openedArchive.Add(targetKirapack, zip);
            }

            lastAccessTime[targetKirapack] = DateTime.Now;

            byte[] buffer;

            var entry = zip.GetEntry(fileName);

            using (var sr = new BinaryReader(entry.Open()))
            {
                buffer = sr.ReadBytes((int)entry.Length);
            }

            return buffer;
        }

        public MemoryStream ReadStream(string fileName)
        {
            var bytes = Read(fileName);
            return new MemoryStream(bytes);
        }

        public string ReadString(string fileName, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var bytes = Read(fileName);

            return encoding.GetString(bytes);
        }

        public Texture2D ReadTexture2D(string fileName)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(Read(fileName));
            tex.wrapMode = TextureWrapMode.Mirror;

            return tex;
        }

        /// <summary>
        /// Write file to fs root path, not any kirapack
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>True if sucess</returns>
        public bool Write(string fileName, byte[] data)
        {
            try
            {
                File.WriteAllBytes(KiraPath.Combine(root, fileName), data);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save a kirapack with files from filesystem
        /// </summary>
        /// <param name="kiraPack"></param>
        /// <param name="fileList"></param>
        /// <returns>True if sucess</returns>
        public bool SaveKirapack(string kiraPack, string[] fileList)
        {
            try
            {
                using (var zip = ZipFile.Open(kiraPack, ZipArchiveMode.Create))
                {
                    foreach (var file in fileList)
                    {
                        var entry = zip.CreateEntry(file, Compression.CompressionLevel.Fastest);
                        using (var bw = new BinaryWriter(entry.Open()))
                        {
                            var bytes = Read(file);

                            bw.Write(bytes);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string Extract(string fileName, bool force = false)
        {
            var bytes = Read(fileName);

            var md5Filename = "";

            using (var md5 = MD5.Create())
            {
                var tmp = Encoding.UTF8.GetBytes(index[fileName] + fileName);
                var arr = md5.ComputeHash(tmp);

                md5Filename = Convert.ToBase64String(arr);
                md5Filename = md5Filename.Replace("/","_");
            }
                

            var path = KiraPath.Combine(tempPath, md5Filename + Path.GetExtension(fileName));
            var write = true;

            if (File.Exists(path))
                if (force)
                    File.Delete(path);
                else
                    write = false;

            if (write)
                File.WriteAllBytes(path, bytes);

            return path;
        }

        //public void ReleaseUnusedKirapacks() => (from x in openedArchive where DateTime.Now - lastAccessTime[x.Key] > TimeSpan.FromMinutes(1) select x.Key).All((key) =>
        //{
        //    openedArchive[key].Dispose();
        //    openedArchive.Remove(key);

        //    lastAccessTime.Remove(key);
        //    return true;
        //});

        public void ReleaseUnusedKirapacks()
        {
            List<string> removeKey = new List<string>();
            foreach (var ar in openedArchive)
            {
                if (DateTime.Now - lastAccessTime[ar.Key] > TimeSpan.FromMinutes(1))
                    removeKey.Add(ar.Key);
            }
            for (int i = 0; i < removeKey.Count; i++)
            {
                //Debug.Log($"Release pack:{removeKey[i]}");
                openedArchive[removeKey[i]].Dispose();
                openedArchive.Remove(removeKey[i]);
                lastAccessTime.Remove(removeKey[i]);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposed = true;

                    foreach (var kv in openedArchive)
                    {
                        kv.Value.Dispose();
                    }

                    openedArchive = null;
                    lastAccessTime = null;

                    SaveIndex();
                    index = null;
                }

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~KiraFilesystem()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
*/
