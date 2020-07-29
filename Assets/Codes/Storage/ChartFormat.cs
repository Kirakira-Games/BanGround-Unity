#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using System.Linq;
using UnityEngine.Scripting;
using ProtoBuf.Meta;

[JsonConverter(typeof(StringEnumConverter))]
[ProtoContract()]
public enum Language
{
    English = 0,
    SimplifiedChinese,
    TraditionalChinese,
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

    [ProtoMember(7, IsRequired = false)]
    [System.ComponentModel.DefaultValue(float.NaN)]
    public float x { get; set; } = float.NaN;

    [ProtoMember(8, IsRequired = false)]
    [System.ComponentModel.DefaultValue(0)]
    public float y { get; set; } = 0;
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

    private List<int> cachedDifficultyLevel;
    public List<int> difficultyLevel
    {
        get
        {
            if (cachedDifficultyLevel == null)
            {
                cachedDifficultyLevel = new List<int>();

                for (var diff = Difficulty.Easy; diff <= Difficulty.Special; diff++)
                {
                    var chart = DataLoader.GetChartPath(sid, diff);
                    cachedDifficultyLevel.Add(DataLoader.GetChartLevel(chart));
                }
            }
            return cachedDifficultyLevel;
        }
    }

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
    public List<PlayResult> resultsList { get; set; } = new System.Collections.Generic.List<PlayResult>();

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

    public static T LoadFromKiraFs<T>(string path) where T : IExtensible
    {
        var stream = KiraFilesystem.Instance.ReadStream(path);
        return Serializer.Deserialize<T>(stream);
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