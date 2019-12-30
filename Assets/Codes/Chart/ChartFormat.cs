using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum Difficulty
{
	Easy = 0,
	Normal,
	Hard,
	Expert,
	Special
};

enum NoteType
{
	BPM = 0,
	Single,
	Flick,
	SlideTick,
	SlideTickEnd,
};

class Beat
{
	int Number;
	int Numerator;
	int Denominator;
}

class Note
{
	NoteType Type;
	Beat Beat;
	int Lane;
	int TickStack;
	float Value;
};

class Chart
{
	Difficulty Difficulty;
	byte Level;
	int NumNotes;
	List<Note> Notes;
};

class Header
{
	string Title;
	string Artist;
	string ChartAuthor;

	string TitleUnicode;
	string ArtistUnicode;
	string ChartAuthorUnicode;

	bool BackgroundIsVideo;

	float PreviewStart;
	float PreviewEnd;

	short NumCharts;
	List<Chart> Charts;
};