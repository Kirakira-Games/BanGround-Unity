using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace System.IO
{
    class KiraFilesystem
    {
        public static KiraFilesystem Instance;

        // file, kirapack
        Dictionary<string, string> index;
        string indexFile;
        string root;
        public KiraFilesystem(string indexFile, string filesystemRoot)
        {
            if (Instance == null)
                Instance = this;

            this.indexFile = indexFile;
            root = filesystemRoot;

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
        }

        public void AddToIndex(string kiraPack)
        {
            using (var s = File.OpenRead(kiraPack))
            {
                using (var zip = new ZipArchive(s))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (index.ContainsKey(entry.FullName))
                            index.Remove(entry.FullName);

                        index.Add(entry.FullName, kiraPack);
                    }
                }
            }

            index.Add(kiraPack, kiraPack);
        }

        public void RemoveFromIndex(string kiraPack)
        {
            var entries = (from x in index where x.Value == kiraPack select x.Key).ToArray();

            foreach (var entry in entries)
                index.Remove(entry);

            index.Remove(kiraPack);
        }

        public void RemoveFileFromIndex(string fileName)
        {
            var path = Path.Combine(root, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if(index.ContainsKey(fileName))
            {
                index.Remove(fileName);
            }
        }

        public void CleanUnusedKirapack()
        {

        }

        public void SaveIndex()
        {
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

        private void GetFiles(List<string> files, DirectoryInfo di)
        {
            var subfiles = from x in di.GetFiles()
                        select x.FullName.Replace(root, "");

            var subdirs = di.GetDirectories();

            files.AddRange(subfiles);

            foreach (var sdi in subdirs)
                GetFiles(files, sdi);
        }

        public string[] ListFiles()
        {
            var files = index.Keys.ToList();
            GetFiles(files, new DirectoryInfo(root));
            return files.ToArray();
        }

        public bool Exists(string fileName)
        {
            if (File.Exists(Path.Combine(root, fileName)))
                return true;

            if (!index.ContainsKey(fileName))
                return false;

            var targetKirapack = index[fileName];

            if (!File.Exists(targetKirapack))
                return false;

            using (var zip = ZipFile.OpenRead(targetKirapack))
            {
                if (zip.GetEntry(fileName) != null)
                    return true;
            }

            return false;
        }

        public byte[] Read(string fileName)
        {
            if (File.Exists(Path.Combine(root, fileName)))
            {
                return File.ReadAllBytes(Path.Combine(root, fileName));
            }

            if (!index.ContainsKey(fileName))
                throw new FileNotFoundException($"File {fileName} not found in filesystem.");

            var targetKirapack = index[fileName];

            byte[] buffer;

            using (var zip = ZipFile.OpenRead(targetKirapack))
            {
                var entry = zip.GetEntry(fileName);

                using (var sr = new BinaryReader(entry.Open()))
                {
                    buffer = sr.ReadBytes((int)entry.Length);
                }
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

            return tex;
        }
    }
}
