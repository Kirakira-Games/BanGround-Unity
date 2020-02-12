#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;

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

}

[ProtoContract()]
public partial class Chart : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    [System.ComponentModel.DefaultValue("")]
    public string author { get; set; } = "";

    [ProtoMember(2)]
    [System.ComponentModel.DefaultValue("")]
    public string authorUnicode { get; set; } = "";

    [ProtoMember(3)]
    [System.ComponentModel.DefaultValue("")]
    public string backgroundFile { get; set; } = "";

    [ProtoMember(4)]
    public Difficulty difficulty { get; set; }

    [ProtoMember(5)]
    [System.ComponentModel.DefaultValue("")]
    public string fileName { get; set; } = "";

    [ProtoMember(6)]
    public byte level { get; set; }

    [ProtoMember(7)]
    public int offset { get; set; }

    [ProtoMember(8)]
    public System.Collections.Generic.List<Note> notes = new System.Collections.Generic.List<Note>();

}

[ProtoContract()]
public partial class Header : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    [System.ComponentModel.DefaultValue("")]
    public string Title { get; set; } = "";

    [ProtoMember(2)]
    [System.ComponentModel.DefaultValue("")]
    public string Artist { get; set; } = "";

    [ProtoMember(3)]
    [System.ComponentModel.DefaultValue("")]
    public string TitleUnicode { get; set; } = "";

    [ProtoMember(4)]
    [System.ComponentModel.DefaultValue("")]
    public string ArtistUnicode { get; set; } = "";

    [ProtoMember(5, IsPacked = true)]
    public float[] Preview { get; set; }

    [ProtoMember(6)]
    [System.ComponentModel.DefaultValue("")]
    public string DirName { get; set; } = "";

    [ProtoMember(7)]
    public System.Collections.Generic.List<Chart> charts = new System.Collections.Generic.List<Chart>();

}

[ProtoContract()]
public partial class SongList : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    [System.ComponentModel.DefaultValue("")]
    public string GenerateDate { get; set; } = "";

    [ProtoMember(2)]
    public System.Collections.Generic.List<Header> songs = new System.Collections.Generic.List<Header>();

    public SongList(string date, List<Header> list)
    {
        songs = list;
        GenerateDate = date;
    }

}

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
    [System.ComponentModel.DefaultValue("")]
    public string FolderName { get; set; } = "";

    [ProtoMember(6)]
    [System.ComponentModel.DefaultValue("")]
    public string ChartName { get; set; } = "";

}

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
            string json = File.ReadAllText(LiveSetting.scoresPath);
            return JsonConvert.DeserializeObject<PlayRecords>(json);
        }
        else
        {
            return new PlayRecords();
        }
    }

    public static string SaveRecord(PlayRecords a)
    {
        string json = JsonConvert.SerializeObject(a);
        File.WriteAllText(LiveSetting.scoresPath,json);
        return json;
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006