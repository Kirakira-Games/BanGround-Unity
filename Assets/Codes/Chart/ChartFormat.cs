using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Difficulty
{
	Easy = 0 ,
	Normal = 1 ,
	Hard = 2 ,
	Expert = 3 ,
	Special = 4
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
    public string fileName;//for load in usage
    public byte level;
    // TODO: Fix this delay
    public int offset = 0;
    public List<Note> notes;
    public Chart(string _auther,byte _level,Difficulty _diff,string _fileName)
    {
        authorUnicode = _auther;
        level = _level;
        difficulty = _diff;
        fileName = _fileName;
    }
};

public class Header
{
    public string Title;
    public string Artist;

    public string TitleUnicode;
    public string ArtistUnicode;

    //public float PreviewStart;
    //public float PreviewEnd;
    public float[] Preview;

    public short NumCharts;
    public string DirName;//for load in usage

    public List<Chart> charts;
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
    public string FolderName;
    public string ChartName;
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

public class SongList
{
    public string GenerateDate;
    public List<Header> songs = new List<Header>();
    public SongList(string date,List<Header> list)
    {
        songs = list;
        GenerateDate = date;
    }
}