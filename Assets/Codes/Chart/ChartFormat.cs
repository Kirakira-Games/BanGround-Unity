using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Difficulty
{
	Easy = 0,
	Normal,
	Hard,
	Expert,
	Special
};

public enum NoteType
{
	BPM = 0,
	Single,
	Flick,
	SlideTick,
	SlideTickEnd,
};

public class Note
{
    public NoteType Type;
    public int[] Beat;
    public int Lane;
    public int TickStack;
    public float Value;
};

public class Chart
{
    public string Author;
    public string AuthorUnicode;
    public string BackgroundFile;
    public Difficulty Difficulty;
    public byte Level;
    public int NumNotes;
    public List<Note> Notes;
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