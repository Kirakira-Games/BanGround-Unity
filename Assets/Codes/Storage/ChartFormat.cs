#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using System.Linq;
using UnityEngine.Scripting;
using BanGround;

public enum Language
{
    English = 0,
    SimplifiedChinese,
    TraditionalChinese,
    Mars,
    Japanese,
    Korean,
    Czech,
    Danish,
    Dutch,
    Finnish,
    French,
    German,
    Greek,
    Hungarian,
    Italian,
    Bulgarion, 
    Koreana,
    Norwegian,
    Polish,
    Portuguese,
    Russian,
    Spanish,
    Swedish,
    Thai,
    Turkish,
    AutoDetect = -1 
}

public static class DifficultyUtil
{
    public static string Lower(this V2.Difficulty difficulty)
    {
        return difficulty.ToString().ToLower();
    }
}

public enum ClearMarks
{
    AP = 0,
    FC = 1,
    CL = 2,
    F = 3,
}

public enum Ranks
{
    SSS = 0,
    SS = 1,
    S = 2,
    A = 3,
    B = 4,
    C = 5,
    D = 6,
    F = 7,
}

[Obsolete("BanGround has migrated to new file structure. These old protobufs are not used anymore.")]
[Preserve]
[ProtoContract()]
public partial class NoteAnim : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1, IsPacked = true)]
    public int[] beat { get; set; }

    [ProtoMember(2, IsRequired = false)]
    [System.ComponentModel.DefaultValue(float.NaN)]
    public float speed { get; set; } = float.NaN;

    [ProtoMember(3, IsRequired = false)]
    [System.ComponentModel.DefaultValue(float.NaN)]
    public float lane { get; set; } = float.NaN;

    [ProtoMember(4, IsRequired = false)]
    [System.ComponentModel.DefaultValue(float.NaN)]
    public float y { get; set; } = float.NaN;
}

[Obsolete("BanGround has migrated to new file structure. These old protobufs are not used anymore.")]
[Preserve]
[Serializable]
[ProtoContract()]
public partial class Note : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public V2.NoteType type { get; set; }

    [ProtoMember(2, IsPacked = true)]
    public int[] beat { get; set; }

    [ProtoMember(3)]
    public int lane { get; set; }

    [ProtoMember(4)]
    [System.ComponentModel.DefaultValue(-1)]
    public int tickStack { get; set; } = -1;

    [ProtoMember(5)]
    public float value { get; set; }

    [ProtoMember(6)]
    public List<NoteAnim> anims { get; } = new List<NoteAnim>();

    [ProtoMember(7, IsRequired = false)]
    [System.ComponentModel.DefaultValue(float.NaN)]
    public float x { get; set; } = float.NaN;

    [ProtoMember(8, IsRequired = false)]
    [System.ComponentModel.DefaultValue(0)]
    public float y { get; set; } = 0;
}

[Obsolete("BanGround has migrated to new file structure. These old protobufs are not used anymore.")]
[Preserve]
[ProtoContract()]
public partial class Chart : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public V2.Difficulty Difficulty { get; set; }

    [ProtoMember(2)]
    public int level { get; set; }

    [ProtoMember(3)]
    public int offset { get; set; }


    [ProtoMember(4)]
    public List<Note> notes { get; set; } = new List<Note>();

}
[Preserve]
public static class ProtobufHelper
{
    public static void Write(object data, IFile target)
    {
        byte[] result;
        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, data);
            result = stream.ToArray();
        }
        using (var stream = target.Open(FileAccess.Write))
        {
            stream.SetLength(result.Length);
            stream.Write(result, 0, result.Length);
        }
    }

    public static void Save(object data, string path)
    {
        if (File.Exists(path))
        {
            using (var file = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.None)) 
            {
                Serializer.Serialize(file, data);
            }
        }
        else
        {
            using (var file = File.Create(path))
            {
                Serializer.Serialize(file, data);
            }
        }
    }

    public static T Load<T>(IFile file) where T : IExtensible
    {
        using (var stream = file.Open(FileAccess.Read))
        {
            return Serializer.Deserialize<T>(stream);
        }
    }

    public static T Load<T>(string path) where T : IExtensible
    {
        if (File.Exists(path))
        {
            using (var file = File.OpenRead(path))
            {
                return Serializer.Deserialize<T>(file);
            }
        }
        else
        {
            return default;
        }
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006