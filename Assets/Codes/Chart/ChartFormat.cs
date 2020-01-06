using System;
using System.Collections.Generic;
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
};