#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using UnityEngine.Scripting;

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum Language
{
    English = 0,
    SimplifiedChinese,
    TraditionalChinese,
    Japanese,
    Bulgarion,
    Czech,
    Danish,
    Dutch,
    Finnish,
    French,
    German,
    Greek,
    Hungarian,
    Italian,
    Korean,
    Koreana,
    Norwegian,
    Polish,
    Portuguese,
    Russian,
    Spanish,
    Swedish,
    Thai,
    Turkish
}

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum Difficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
    Expert = 3,
    Special = 4,
}

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum NoteType
{
    [ProtoEnum(Name = @"BPM")]
    BPM = 0,
    Single = 1,
    Flick = 2,
    SlideTick = 3,
    SlideTickEnd = 4,
}

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum ClearMarks
{
    [ProtoEnum(Name = @"AP")]
    AP = 0,
    [ProtoEnum(Name = @"FC")]
    FC = 1,
    [ProtoEnum(Name = @"CL")]
    CL = 2,
    F = 3,
}

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum Ranks
{
    [ProtoEnum(Name = @"SSS")]
    SSS = 0,
    [ProtoEnum(Name = @"SS")]
    SS = 1,
    S = 2,
    A = 3,
    B = 4,
    C = 5,
    D = 6,
    F = 7,
}

[Preserve]
[ProtoContract()]
public partial class NoteAnim : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1, IsPacked = true)]
    public int[] beat { get; set; }

    [ProtoMember(2)]
    public float speed { get; set; }
}

[Preserve]
[Serializable]
[ProtoContract()]
public partial class Note : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public NoteType type { get; set; }

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

}

[Preserve]
[ProtoContract()]
public partial class BackgroundFile : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    [global::System.ComponentModel.DefaultValue("")]
    public string pic { get; set; } = "";

    [ProtoMember(2)]
    [global::System.ComponentModel.DefaultValue("")]
    public string vid { get; set; } = "";

}

[Preserve]
[ProtoContract()]
public partial class Chart : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public Difficulty Difficulty { get; set; }

    [ProtoMember(2)]
    public int level { get; set; }

    [ProtoMember(3)]
    public int offset { get; set; }


    [ProtoMember(4)]
    public List<Note> notes { get; set; } = new List<Note>();

}

[Preserve]
[ProtoContract()]
public partial class cHeader : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public int version { get; set; }

    [ProtoMember(2)]
    public int sid { get; set; }

    [ProtoMember(3)]
    public int mid { get; set; }

    [ProtoMember(4)]
    [global::System.ComponentModel.DefaultValue("")]
    public string author { get; set; } = "";

    [ProtoMember(5)]
    [global::System.ComponentModel.DefaultValue("")]
    public string authorNick { get; set; } = "";

    [ProtoMember(6)]
    public BackgroundFile backgroundFile { get; set; }

    [ProtoMember(7, IsPacked = true)]
    public float[] preview { get; set; }

    [ProtoMember(8)]
    public List<string> tag { get; set; } = new List<string>();

    [ProtoMember(9)]
    public List<int> difficultyLevel { get; set; } = new List<int>();

}

[Preserve]
[ProtoContract()]
public partial class mHeader : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public int mid { get; set; }

    [ProtoMember(2)]
    [global::System.ComponentModel.DefaultValue("")]
    public string title { get; set; } = "";

    [ProtoMember(3)]
    [global::System.ComponentModel.DefaultValue("")]
    public string artist { get; set; } = "";

    [ProtoMember(4, IsPacked = true)]
    public float[] preview { get; set; }

    [ProtoMember(5, IsPacked = true)]
    public float[] BPM { get; set; }

    [ProtoMember(6)]
    public float length { get; set; }

}


[Preserve]
[ProtoContract()]
public partial class SongList : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public List<cHeader> cHeaders { get; set; } = new List<cHeader>();

    [ProtoMember(2)]
    public List<mHeader> mHeaders { get; set; } = new List<mHeader>();
}

[Preserve]
[ProtoContract()]
public partial class PlayResult : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public ClearMarks clearMark { get; set; }

    [ProtoMember(2)]
    public Ranks ranks { get; set; }

    [ProtoMember(3)]
    public double Score { get; set; }

    [ProtoMember(4)]
    public double Acc { get; set; }

    [ProtoMember(5)]
    public int ChartId { get; set; }

    [ProtoMember(6)]
    public Difficulty Difficulty { get; set; }

}

[Preserve]
[ProtoContract()]
public partial class PlayRecords : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public System.Collections.Generic.List<PlayResult> resultsList { get; set; } = new System.Collections.Generic.List<PlayResult>();

    public PlayRecords()
    {
        resultsList = new List<PlayResult>();
    }

    public static PlayRecords OpenRecord()
    {
        if (File.Exists(LiveSetting.scoresPath))
        {
            return ProtobufHelper.Load<PlayRecords>(LiveSetting.scoresPath);
        }
        else
        {
            return new PlayRecords();
        }
        
    }

    public static string SaveRecord(PlayRecords a)
    {
        ProtobufHelper.Save(a, LiveSetting.scoresPath);
        string json = JsonConvert.SerializeObject(a);
        //File.WriteAllText(LiveSetting.scoresPath,json);
        return json;
    }
}

[Preserve]
public static class ProtobufHelper
{
    public static void Save<T>(T data, string path) where T : IExtensible
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

public class cHeaderComparer : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        return x.sid - y.sid;
    }
}