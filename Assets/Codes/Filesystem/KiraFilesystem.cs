﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BanGround
{
    class PreventDuplicateFileSet : IEnumerable<IFile>
    {
        private Dictionary<string, IFile> mFilesDic = new Dictionary<string, IFile>();
        private List<IFile> mFileToDelete = new List<IFile>();
        public int Count => mFilesDic.Count;

        public PreventDuplicateFileSet() { }
        public PreventDuplicateFileSet(IEnumerable<IFile> files)
        {
            foreach (var file in files)
                Add(file);
        }

        public void DeleteDuplicateFile()
        {
            foreach (var file in mFileToDelete)
                file.Delete();
        }

        public void Add(IFile file)
        {
            if (file == null)
                return;
            if (mFilesDic.TryGetValue(file.Name, out var current))
            {
                if (current.LastModified < file.LastModified)
                {
                    mFilesDic[file.Name] = file;
                    mFileToDelete.Add(current);
                }
                else
                {
                    mFileToDelete.Add(file);
                }
            }
            else
            {
                mFilesDic.Add(file.Name, file);
            }
        }

        public void Remove(IFile file)
        {
            if (mFilesDic.ContainsKey(file.Name))
                mFilesDic.Remove(file.Name);
        }

        public IEnumerator<IFile> GetEnumerator()
        {
            return mFilesDic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class NormalFile : IFile
    {
        private FileInfo internalInfo;
        private Stream openedStream;

        public string Name {
            get => internalInfo.FullName.Replace('\\', '/').Replace(RootPath, "").TrimStart('/');
            set
            {
                var path = KiraPath.Combine(RootPath, value);
                var dir = KiraPath.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                internalInfo.MoveTo(path);
            }
        }

        public string RootPath { get; }

        public bool Opened => openedStream != null;

        public int Size => (int)internalInfo.Length;

        public DateTimeOffset LastModified => internalInfo.LastWriteTimeUtc;

        public NormalFile(string searchPath, FileInfo info)
        {
            RootPath = searchPath;
            internalInfo = info;
        }

        public bool Delete()
        {
            Debug.Log($"Delete {RootPath} :: {Name}");
            try
            {
                internalInfo.Delete();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
                return false;
            }

            return true;
        }

        public Stream Open(FileAccess access)
        {
            return internalInfo.Open(access.HasFlag(FileAccess.Write) ? FileMode.Truncate : FileMode.Open, access);
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
        internal string __internal_filename;

        Action<PakFile> onDelete;
        Action<string, PakFile> onRename;

        Func<PakFile, FileAccess, ZipArchiveEntry> getEntry;
        private Stream openedStream;

        public PakFile(string fileName, Func<PakFile, FileAccess, ZipArchiveEntry> entry, string pakName, Action<PakFile> onFileDelete, Action<string, PakFile> onFileRename)
        {
            __internal_filename = fileName;
            RootPath = pakName;
            getEntry = entry;
            onDelete = onFileDelete;
            onRename = onFileRename;
        }

        public string Name
        {
            get => __internal_filename.Replace('\\', '/');
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

                var oldName = Name;
                __internal_filename = value;

                onRename(oldName, this);
            }
        }

        public bool Opened => openedStream != null;

        public int Size => (int)getEntry(this, FileAccess.Read).Length;

        public string RootPath { get; }

        public DateTimeOffset LastModified {
            get
            {
                //try
                //{
                    var entry = getEntry(this, FileAccess.Read);
                    return entry.LastWriteTime;
                //}
                //catch(Exception ex)
                //{
                //    throw ex;
                //}
            }
        }

        public bool Delete()
        {
            Debug.Log($"Delete {RootPath} {Name}");
            try
            {
                onDelete(this);
                getEntry(this, FileAccess.ReadWrite).Delete();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
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

            var path = KiraPath.Combine(RootPath + "_extracted/", md5Filename + Path.GetExtension(Name));
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
        List<string> searchPaths = new List<string>
        {
            KiraPath.Combine(Application.persistentDataPath, "data")
        };

        Dictionary<string, IFile> pakFileIndexs = new Dictionary<string, IFile>();
        Dictionary<string, (ZipArchive, DateTime)> packAccessCache = new Dictionary<string, (ZipArchive, DateTime)>();

        public bool Init()
        {
            return true;
        }

        void OnPakFileRename(string oldName, PakFile pakFile)
        {
            pakFileIndexs.Remove(oldName);
            pakFileIndexs.Add(pakFile.__internal_filename.Replace('\\','/'), pakFile);
        }

        void OnPakFileDelete(PakFile pakFile)
        {
            pakFileIndexs.Remove(pakFile.__internal_filename.Replace('\\', '/'));
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

        ZipArchiveEntry GetEntryStub(PakFile pakfile, FileAccess access) => GetArchive(pakfile.RootPath, access).GetEntry(pakfile.__internal_filename);

        private void RemoveEmptyDir(DirectoryInfo dir)
        {
            var subdirs = dir.GetDirectories();
            if (dir.GetFiles().Length == 0 && subdirs.Length == 0)
            {
                dir.Delete();
                return;
            }
            foreach (var d in subdirs)
            {
                RemoveEmptyDir(d);
            }
        }

        public void AddSearchPath(string path, bool removeEmptyDir = true)
        {
            if (Directory.Exists(path))
            {
                searchPaths.Add(path);

                DirectoryInfo di = new DirectoryInfo(path);

                foreach (var fi in di.GetFiles())
                {
                    if (fi.Extension == ".kpak")
                        AddSearchPath(fi.FullName);
                }
                if (removeEmptyDir)
                {
                    foreach (var d in di.GetDirectories())
                    {
                        RemoveEmptyDir(d);
                    }
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
                    {
                        br.Close();
                        File.Delete(path);
                        return;
                    }
                }

                packPaths.Add(path);
                var zip = GetArchive(path, FileAccess.Read);

                if (zip.Entries.Count == 0)
                {
                    File.Delete(path);
                    packPaths.Remove(path);
                    zip.Dispose();
                    return;
                }

                foreach (var entry in zip.Entries)
                {
                    if (entry.Length != 0)
                    {
                        var fileName = entry.FullName;

                        if (pakFileIndexs.ContainsKey(fileName))
                            pakFileIndexs.Remove(fileName);

                        var file = new PakFile(fileName, GetEntryStub, path, OnPakFileDelete, OnPakFileRename);

                        pakFileIndexs.Add(fileName.Replace('\\', '/'), file);
                    }
                }
            }
        }

        public bool FileExists(string filename)
        {
            var result = pakFileIndexs.ContainsKey(filename);

            if (result)
                return result;

            foreach (var searchPath in searchPaths)
            {
                var fullPath = KiraPath.Combine(searchPath, filename);

                result = File.Exists(fullPath);

                if (result)
                    return result;
            }

            return false;
        }

        private void KeepLastModifiedFile(ref IFile current, IFile newfile)
        {
            if (current == null || newfile == null)
            {
                current = current ?? newfile;
            }
            else if (current.LastModified < newfile.LastModified)
            {
                current.Delete();
                current = newfile;
            }
            else
            {
                newfile.Delete();
            }
        }

        public IFile GetFile(string path)
        {
            var result = new PreventDuplicateFileSet();
            foreach (var searchPath in searchPaths)
            {
                var fi = new FileInfo(KiraPath.Combine(searchPath, path));

                if (fi.Exists)
                {
                    result.Add(new NormalFile(searchPath, fi));
                }
            }
            
            if (pakFileIndexs.ContainsKey(path))
                    result.Add(pakFileIndexs[path]);


            if (result.Count == 0)
                throw new FileNotFoundException("Target file not found in any search path!");

            result.DeleteDuplicateFile();

            return result.ElementAt(0);
        }

        public void RemoveSearchPath(string path)
        {
            if (searchPaths.Contains(path))
                searchPaths.Remove(path);

            if (packPaths.Contains(path))
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

        private void GetFiles(string searchPath, PreventDuplicateFileSet files, DirectoryInfo di, Func<IFile, bool> func)
        {
            foreach (var fi in di.GetFiles())
            {
                var file = new NormalFile(searchPath, fi);

                if (func(file))
                    files.Add(file);
            }

            var subdirs = di.GetDirectories();

            foreach (var sdi in subdirs)
            {
                // exclude replay folder
                if (sdi.Name == "replay")
                    continue;

                GetFiles(searchPath, files, sdi, func);
            }
        }

        public IEnumerable<IFile> Find(Func<IFile, bool> cmp)
        {
            var indexResult = pakFileIndexs.Values.Where(cmp);
            var result = new PreventDuplicateFileSet(indexResult);

            foreach (var searchPath in searchPaths)
            {
                GetFiles(searchPath, result, new DirectoryInfo(searchPath), cmp);
            }

            result.DeleteDuplicateFile();
            return result;
        }

        public void FlushPak(string pakName)
        {
            if (packAccessCache.ContainsKey(pakName))
            {
                var archive = packAccessCache[pakName].Item1;

                archive.Dispose();
                packAccessCache.Remove(pakName);
            }
        }

        public IFile NewFile(string name, string searchPath = null)
        {
            if (searchPath != null && packPaths.Contains(searchPath))
            {
                ZipArchive archive = GetArchive(searchPath, FileAccess.ReadWrite);

                archive.CreateEntry(name);

                return new PakFile(name, GetEntryStub, searchPath, OnPakFileDelete, OnPakFileRename);
            }
            else if (searchPath != null && searchPaths.Contains(searchPath))
            {
                var fi = new FileInfo(KiraPath.Combine(searchPath, name));
                fi.Create().Close();

                return new NormalFile(searchPath, fi);
            }
            else
            {
                searchPath = searchPaths[0];
                var fi = new FileInfo(KiraPath.Combine(searchPath, name));

                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                fi.Create().Close();

                return new NormalFile(searchPath, fi);
            }
        }

        public int RemoveFolder(string path, string pakName = null)
        {
            IEnumerable<IFile> affectedFiles = null;

            if (pakName == null)
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
                if (DateTime.Now - time > onemin)
                {
                    archive.Dispose();
                    packAccessCache.Remove(path);
                }
            }
        }

        public IFile GetOrNewFile(string path)
        {
            if (FileExists(path))
                return GetFile(path);
            return NewFile(path);
        }

        public string[] GetSearchPatchs() => searchPaths.ToArray();
    }
}
