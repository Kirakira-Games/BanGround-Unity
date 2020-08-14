using UnityEngine;

public enum NoteStyle
{
    Circle,
    Cube,
    Dark,
    Custom
}

public enum SEStyle
{
    None,
    Drum,
    Bbben,
    AECBanana,
    Custom
}

public interface IResourceLoader
{
    T LoadSkinResource<T>(string path) where T : Object;
    T LoadSkinResource<T>(string path, NoteStyle style) where T : Object;
    T LoadResource<T>(string path) where T : Object;
    T LoadSEResource<T>(string path, SEStyle style) where T : Object;
    T LoadSEResource<T>(string path) where T : Object;
    T LoadIconResource<T>(string path) where T : Object;
}