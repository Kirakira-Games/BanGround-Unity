using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BanGround
{
    class NormalFile : IFile
    {
        private FileInfo internalInfo;
        private Stream openedStream;

        public string Name { get => internalInfo.FullName.Replace(RootPath, "").Replace('\\','/'); set => internalInfo.MoveTo(Path.Combine(RootPath, value)); }

        public string RootPath { get; }

        public bool Opened => openedStream != null;

        public int Size => (int)internalInfo.Length;

        public NormalFile(string searchPath, FileInfo info)
        {
            RootPath = searchPath;
            internalInfo = info;
        }

        public bool Delete()
        {
            try
            {
                internalInfo.Delete();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public Stream Open(FileAccess access)
        {
            return internalInfo.Open(FileMode.Open, access);
        }

        public byte[] ReadToEnd()
        {
            return File.ReadAllBytes(internalInfo.FullName);
        }

        public bool WriteBytes(byte[] content)
        {
            try
            {
                File.WriteAllBytes(internalInfo.FullName, content);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string Extract(bool force = false) => internalInfo.FullName;
    }

    class PakFile : IFile
    {
        string fileName;

        Action<IFile> onDelete;
        Func<IFile, FileAccess, ZipArchiveEntry> getEntry;
        private Stream openedStream;

        public PakFile(string fileName, Func<IFile, FileAccess, ZipArchiveEntry> entry, string pakName, Action<IFile> onFileDelete)
        {
            this.fileName = fileName;
            RootPath = pakName;
            getEntry = entry;
            onDelete = onFileDelete;
        }

        public string Name 
        { 
            get => fileName; 
            set 
            {
                var archive = getEntry(this, FileAccess.ReadWrite).Archive;
                var newEntry = archive.CreateEntry(value);

                if (openedStream == null)
                    openedStream = getEntry(this, FileAccess.ReadWrite).Open();

                using (var newStream = newEntry.Open())
                    openedStream.CopyTo(newStream);
                    
                openedStream.Close();
                openedStream = null;

                getEntry(this, FileAccess.ReadWrite).Delete();
                fileName = value;
            }
        }

        public bool Opened => openedStream != null;

        public int Size => (int)getEntry(this, FileAccess.Read).Length;

        public string RootPath { get; }

        public bool Delete()
        {
            try
            {
                onDelete(this);
                getEntry(this, FileAccess.ReadWrite).Delete();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public Stream Open(FileAccess access)
        {
            if (access == FileAccess.Write)
                access = FileAccess.ReadWrite;

            return getEntry(this, access).Open();
        }

        public byte[] ReadToEnd()
        {
            byte[] buffer;

            using (var br = new BinaryReader(getEntry(this, FileAccess.Read).Open()))
            {
                buffer = br.ReadBytes((int)getEntry(this, FileAccess.Read).Length);
            }

            return buffer;
        }

        public bool WriteBytes(byte[] content)
        {
            using (var bw = new BinaryWriter(getEntry(this, FileAccess.ReadWrite).Open()))
            {
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write(content);
            }

            return true;
        }
        
        public string Extract(bool force = false)
        {
            var bytes = ReadToEnd();

            var md5Filename = "";

            using (var md5 = MD5.Create())
            {
                var tmp = Encoding.UTF8.GetBytes(Name);
                var arr = md5.ComputeHash(tmp);

                md5Filename = Convert.ToBase64String(arr);
                md5Filename = md5Filename.Replace("/", "_");
            }

            if (!Directory.Exists(RootPath + "_extracted/"))
                Directory.CreateDirectory(RootPath + "_extracted/");

            var path = Path.Combine(RootPath + "_extracted/" , md5Filename + Path.GetExtension(fileName));
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
    }

    class KiraFilesystem : IFileSystem
    {
        List<string> packPaths = new List<string>();
        List<string> searchPaths = new List<string>();

        Dictionary<string, IFile> pakFileIndexs = new Dictionary<string, IFile>();
        Dictionary<string, (ZipArchive, DateTime)> packAccessCache = new Dictionary<string, (ZipArchive, DateTime)>();

        public bool Init()
        {
            return true;
        }

        void OnPakFileDelete(IFile pakFile)
        {
            pakFileIndexs.Remove(pakFile.Name);
        }

        ZipArchive GetArchive(string path, FileAccess access)
        {
            if (!packPaths.Contains(path))
                throw new FileNotFoundException("Target kirapack already removed!");

            if (packAccessCache.ContainsKey(path))
            {
                var (archive, _) = packAccessCache[path];
                if (access == FileAccess.Read && archive.Mode == ZipArchiveMode.Update)
                {
                    archive.Dispose();
                    archive = ZipFile.OpenRead(path);
                }
                else if (access == FileAccess.ReadWrite && archive.Mode == ZipArchiveMode.Read)
                {
                    archive.Dispose();
                    archive = ZipFile.Open(path, ZipArchiveMode.Update);
                }

                packAccessCache[path] = (archive, DateTime.Now);
                return archive;
            }

            ZipArchive newArchive = null;

            if (access == FileAccess.Read)
            {
                newArchive = ZipFile.OpenRead(path);
            }
            else if (access == FileAccess.ReadWrite)
            {
                newArchive = ZipFile.Open(path, ZipArchiveMode.Update);
            }

            packAccessCache.Add(path, (newArchive, DateTime.Now));
            return newArchive;
        }

        ZipArchiveEntry GetEntryStub(IFile pakfile, FileAccess access) => GetArchive(pakfile.RootPath, access).GetEntry(pakfile.Name);

        public void AddSearchPath(string path)
        {
            if(Directory.Exists(path))
            {
                searchPaths.Add(path);

                DirectoryInfo di = new DirectoryInfo(path);

                foreach(var fi in di.GetFiles())    
                {
                    if (fi.Extension == ".kpak")
                        AddSearchPath(fi.FullName);
                }

                return;
            }

            if (File.Exists(path))
            {
                using (var br = new BinaryReader(File.OpenRead(path)))
                {
                    ushort pkMagic = br.ReadUInt16();
                    byte version = br.ReadByte();

                    if (!(pkMagic == 0x4b50 && version == 3))
                        return;
                }

                packPaths.Add(path);

                var zip = GetArchive(path, FileAccess.Read);

                foreach (var entry in zip.Entries)
                {
                    if (entry.Length != 0)
                    {
                        if (pakFileIndexs.ContainsKey(entry.FullName))
                            pakFileIndexs.Remove(entry.FullName);

                        var file = new PakFile(entry.FullName, GetEntryStub, path, OnPakFileDelete);

                        pakFileIndexs.Add(entry.FullName, file);
                    }
                }
            }
        }

        public bool FileExists(string filename)
        {
            var result = pakFileIndexs.ContainsKey(filename);

            if (result)
                return result;

            foreach(var searchPath in searchPaths)
            {
                var fullPath = Path.Combine(searchPath, filename);

                result = File.Exists(fullPath);

                if (result)
                    return result;
            }

            return false;
        }

        public IFile GetFile(string path)
        {
            foreach(var searchPath in searchPaths)
            {
                var fi = new FileInfo(Path.Combine(searchPath, path));

                if(fi.Exists)
                {
                    return new NormalFile(searchPath, fi);
                }
            }

            if (pakFileIndexs.ContainsKey(path))
                return pakFileIndexs[path];

            throw new FileNotFoundException("Target file not found in any search path!");
        }

        public void RemoveSearchPath(string path)
        {
            if (searchPaths.Contains(path))
                searchPaths.Remove(path);

            if(packPaths.Contains(path))
            {
                packPaths.Remove(path);

                var files = pakFileIndexs.Where(kvp => kvp.Value.RootPath == path).Select(kvp => kvp.Key).ToArray();

                foreach (var file in files)
                    pakFileIndexs.Remove(file);

                if (packAccessCache.ContainsKey(path))
                {
                    var (pack, time) = packAccessCache[path];

                    packAccessCache.Remove(path);

                    pack.Dispose();
                }
            }
        }

        private void GetFiles(string searchPath, List<IFile> files, DirectoryInfo di, Func<IFile, bool> func)
        {
            foreach(var fi in di.GetFiles())
            {
                var file = new NormalFile(searchPath, fi);

                if (func(file))
                    files.Add(file);
            }

            var subdirs = di.GetDirectories();

            foreach (var sdi in subdirs)
                GetFiles(searchPath, files, sdi, func);
        }

        public IEnumerable<IFile> Find(Func<IFile, bool> cmp)
        {
            var indexResult = pakFileIndexs.Values.Where(cmp).ToList();
            var filesystemResult = new List<IFile>();

            foreach(var searchPath in searchPaths)
            {
                GetFiles(searchPath, filesystemResult, new DirectoryInfo(searchPath), cmp);
            }

            if (false)
            {
                indexResult.AddRange(filesystemResult.Where((f) => indexResult.Find(f1 => f1.Name == f.Name) == null));
                return indexResult;
            }
            else
            {
                filesystemResult.AddRange(indexResult.Where((f) => filesystemResult.Find(f1 => f1.Name == f.Name) == null));
                return indexResult;
            }
        }

        public void FlushPak(string pakName)
        {
            if(packAccessCache.ContainsKey(pakName))
            {
                var archive = packAccessCache[pakName].Item1;

                archive.Dispose();
                packAccessCache.Remove(pakName);
            }
        }

        public IFile NewFile(string name, string searchPath = null)
        {
            if(searchPath != null && packPaths.Contains(searchPath))
            {
                ZipArchive archive = GetArchive(searchPath, FileAccess.ReadWrite);

                archive.CreateEntry(name);

                return new PakFile(name, GetEntryStub, searchPath, OnPakFileDelete);
            }
            else if(searchPath != null && searchPaths.Contains(searchPath))
            {
                var fi = new FileInfo(Path.Combine(searchPath, name));
                fi.Create().Close();

                return new NormalFile(searchPath, fi);
            }
            else
            {
                searchPath = searchPaths[0];
                var fi = new FileInfo(Path.Combine(searchPath, name));
                fi.Create().Close();

                return new NormalFile(searchPath, fi);
            }
        }

        public int RemoveFolder(string path, string pakName = null)
        {
            IEnumerable<IFile> affectedFiles = null;
            
            if(pakName == null)
            {
                affectedFiles = Find(file => file.Name.StartsWith(path));
            }
            else
            {
                affectedFiles = Find(file => file.RootPath == pakName && file.Name.StartsWith(path));
            }

            foreach (var file in affectedFiles)
                file.Delete();

            return affectedFiles.Count();
        }

        public int RenameFolder(string from, string to, string pakName)
        {
            IEnumerable<IFile> affectedFiles = null;

            if (pakName == null)
            {
                affectedFiles = Find(file => file.Name.StartsWith(from));
            }
            else
            {
                affectedFiles = Find(file => file.RootPath == pakName && file.Name.StartsWith(from));
            }

            foreach (var file in affectedFiles)
            {
                file.Name = file.Name.Replace(from, to);
            }

            return affectedFiles.Count();
        }

        public bool Shutdown()
        {
            var openedPaks = packAccessCache.Values.Select(t => t.Item1);

            foreach (var archive in openedPaks)
                archive.Dispose();

            packAccessCache.Clear();

            return true;
        }
        public IEnumerator<IFile> GetEnumerator()
        {
            return Find(_ => true).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Find(_ => true).GetEnumerator();
        }

        readonly TimeSpan onemin = TimeSpan.FromMinutes(1);

        public void OnUpdate()
        {
            var paksToRelease = packAccessCache.Where((kvp) => 
            {
                var (_, (_, time)) = kvp;

                if (DateTime.Now - time > onemin)
                    return true;

                return false;
            }).ToArray();

            foreach (var (path, (archive, time)) in paksToRelease)
            {
                if(DateTime.Now - time > onemin)
                {
                    archive.Dispose();
                    packAccessCache.Remove(path);
                }
            }
        }
    }
}
