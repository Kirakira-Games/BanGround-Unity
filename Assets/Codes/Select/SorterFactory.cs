using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zenject;

[JsonConverter(typeof(StringEnumConverter))]
public enum Sorter
{
    ChartDifficulty,
    SongName,
    SongArtist,
    ChartAuthor,
    ChartScore
}


public class ChartDifSort : IComparer<cHeader>
{
    private IDataLoader dataLoader;
    private int difficulty;

    public ChartDifSort(IDataLoader dataLoader, int difficulty)
    {
        this.dataLoader = dataLoader;
        this.difficulty = difficulty;
    }

    public int Compare(cHeader x, cHeader y)
    {
        x.LoadDifficultyLevels(dataLoader);
        y.LoadDifficultyLevels(dataLoader);
        int difX = x.difficultyLevel[difficulty];
        int difY = y.difficultyLevel[difficulty];
        int dif = difX - difY;
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class SongNameSort : IComparer<cHeader>
{
    private IDataLoader dataLoader;
    public SongNameSort(IDataLoader dataLoader)
    {
        this.dataLoader = dataLoader;
    }

    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = dataLoader.GetMusicHeader(x.mid);
        mHeader ym = dataLoader.GetMusicHeader(y.mid);
        int dif = string.Compare(xm.title, ym.title);
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class SongArtistSort : IComparer<cHeader>
{
    private IDataLoader dataLoader;
    public SongArtistSort(IDataLoader dataLoader)
    {
        this.dataLoader = dataLoader;
    }

    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = dataLoader.GetMusicHeader(x.mid);
        mHeader ym = dataLoader.GetMusicHeader(y.mid);
        int dif = string.Compare(xm.artist, ym.artist);
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class ChartAuthorSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        string xauthor = string.IsNullOrWhiteSpace(x.authorNick) ? x.author : x.authorNick;
        string yauthor = string.IsNullOrWhiteSpace(y.authorNick) ? y.author : y.authorNick;
        int dif = string.Compare(xauthor, yauthor);
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class ChartScoreSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        IEnumerable<PlayResult> ListX = PlayRecordDisplay.playRecords.resultsList.Where(o => o.ChartId == x.sid);
        IEnumerable<PlayResult> ListY = PlayRecordDisplay.playRecords.resultsList.Where(o => o.ChartId == y.sid);
        double resultX = 0, resultY = 0;
        if (ListX.Count() > 0) resultX = ListX.Max(o => o.Score);
        if (ListY.Count() > 0) resultY = ListY.Max(o => o.Score);
        int dif = (int)(resultX - resultY);
        return dif == 0 ? x.mid - y.mid : dif;
        //DAMN:The selector could select a different song if score was changed
    }
}

public interface ISorterFactory
{
    IComparer<cHeader> Create();
}

public class SorterFactory : ISorterFactory
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public IComparer<cHeader> Create()
    {
        switch ((Sorter)cl_cursorter)
        {
            case Sorter.ChartDifficulty:
                return new ChartDifSort(dataLoader, cl_lastdiff);
            case Sorter.SongName:
                return new SongNameSort(dataLoader);
            case Sorter.SongArtist:
                return new SongArtistSort(dataLoader);
            case Sorter.ChartAuthor:
                return new ChartAuthorSort();
            case Sorter.ChartScore:
                return new ChartScoreSort();
            default:
                return new SongNameSort(dataLoader);
        }
    }
}