using UnityEngine;
using System.Collections;
using System.IO;
using Zenject;

public class ResourceLoader : IResourceLoader
{
    [Inject(Id = "fs_assetpath")]
    private KVar fs_assetpath;
    [Inject(Id = "fs_iconpath")]
    private KVar fs_iconpath;
    [Inject(Id = "cl_notestyle")]
    private KVar cl_notestyle;
    [Inject(Id = "cl_sestyle")]
    private KVar cl_sestyle;

    public T LoadResource<T>(string path) where T : Object
    {
        return Resources.Load<T>(fs_assetpath + "/" + path);
    }

    public T LoadSkinResource<T>(string path, NoteStyle style) where T : Object
    {
        return LoadResource<T>(style + "/" + path);
    }

    public T LoadSkinResource<T>(string path) where T : Object
    {
        return LoadSkinResource<T>(path, (NoteStyle)cl_notestyle);
    }

    public T LoadSEResource<T>(string path, SEStyle style) where T : Object
    {
        return Resources.Load<T>("SoundEffects/" + style + "/" + path);
    }

    public T LoadSEResource<T>(string path) where T : Object
    {
        return LoadSEResource<T>(path, (SEStyle)cl_sestyle);
    }

    public T LoadIconResource<T>(string path) where T : Object
    {
        string iconpath = fs_iconpath;
        if (iconpath.EndsWith("/"))
        {
            iconpath = iconpath.Remove(iconpath.Length - 1);
            fs_iconpath.Set(iconpath);
        }
        return Resources.Load<T>(iconpath + "/" + path);
    }
}
