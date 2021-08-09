using UnityEngine;
using System.Collections;
using System.IO;
using Zenject;
using BanGround;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

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

    [Inject]
    private IFileSystem fs;

    private readonly Dictionary<string, Texture2D> mTextureCache = new Dictionary<string, Texture2D>();
    private readonly Dictionary<Texture2D, string> mTextureCacheInverse = new Dictionary<Texture2D, string>();

    public ResourceLoader()
    {
        SceneManager.sceneUnloaded += (scene) =>
        {
            if (scene.name == "Loader")
            {
                return;
            }
            UnloadAllTexture();
        };
    }

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

    public Texture2D LoadTextureFromFs(string path)
    {
        if (!mTextureCache.TryGetValue(path, out var tex) || tex == null)
        {
            try
            {
                tex = fs.GetFile(path).ReadAsTexture();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            tex.name = path;
            mTextureCache.Add(path, tex);
            mTextureCacheInverse.Add(tex, path);
        }
        return tex;
    }

    public void UnloadTexture(string path)
    {
        if (mTextureCache.TryGetValue(path, out var tex))
        {
            Object.Destroy(tex);
            mTextureCache.Remove(path);
            mTextureCacheInverse.Remove(tex);
        }
    }

    public void UnloadTexture(Texture2D texture)
    {
        if (mTextureCacheInverse.TryGetValue(texture, out var path))
        {
            UnloadTexture(path);
        }
    }

    public void UnloadAllTexture()
    {
        var paths = mTextureCache.Keys.ToArray();
        foreach (var path in paths)
        {
            UnloadTexture(path);
        }
    }
}
