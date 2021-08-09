using System;
using System.Collections.Generic;
using System.IO;

namespace BanGround
{
    public interface IFile
    {
        string Name { get; set; }
        string RootPath { get; }
        bool Opened { get; }
        int Size { get; }
        DateTimeOffset LastModified { get; }

        Stream Open(FileAccess access);
        byte[] ReadToEnd();
        bool WriteBytes(byte[] content);

        bool Delete();

        string Extract(bool force = false);
    }

    public interface IFileSystem : IEnumerable<IFile>
    {
        bool Init();
        bool Shutdown();

        IFile NewFile(string name, string searchPath = null);

        IEnumerable<IFile> Find(Func<IFile, bool> cmp);
        IEnumerable<IFile> ListDirectory(string directoryName);
        IFile GetOrNewFile(string path);
        IFile GetFile(string path);

        //string[] GetSearchPaths();
        void AddSearchPath(string path);
        void RemoveSearchPath(string path);
        bool FileExists(string filename);

        //void FlushPak(string pakName);
        //int RenameFolder(string from, string to, string pakName = null);
        //int RemoveFolder(string path, string pakName = null);
        //void OnUpdate();
    }
}
