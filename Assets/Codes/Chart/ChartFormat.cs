using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Difficulty
{
	Easy = 0,
	Normal,
	Hard,
	Expert,
	Special
};

[JsonConverter(typeof(StringEnumConverter))]
public enum NoteType
{
	BPM = 0,
	Single,
	Flick,
	SlideTick,
	SlideTickEnd,
};

[Serializable]
public class Note
{
    public NoteType type;
    public int[] beat;
    public int lane;
    public int tickStack = -1;
    public float value;
};

public class Chart
{
    public string author;
    public string authorUnicode;
    public string backgroundFile;
    public Difficulty difficulty;
    public byte level;
    // TODO: Fix this delay
    public int offset = 0;
    public List<Note> notes;
};

public class Header
{
    public string Title;
    public string Artist;

    public string TitleUnicode;
    public string ArtistUnicode;

    public float PreviewStart;
    public float PreviewEnd;

    public short NumCharts;
    public string DirName;
    public Header(string title,string artist,string dirName)
    {
        TitleUnicode = title;
        ArtistUnicode = artist;
        DirName = dirName;
    }
};

[JsonConverter(typeof(StringEnumConverter))]
public enum ClearMarks { AP, FC, CL, F };
[JsonConverter(typeof(StringEnumConverter))]
public enum Ranks { SSS, SS, S, A, B, C, D, F };

public class PlayResult
{
    public ClearMarks clearMark;
    public Ranks ranks;
    public double Score;
    public double Acc;
    public String FolderName;
    public String ChartName;
}

public class PlayRecords {
    public List<PlayResult> resultsList;
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