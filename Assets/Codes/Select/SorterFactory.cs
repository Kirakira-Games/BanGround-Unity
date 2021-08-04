using System.Collections.Generic;
using BanGround.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zenject;
using V2;

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
    private IDatabaseAPI db;
    private Difficulty difficulty;
    public ChartScoreSort(IDatabaseAPI db, Difficulty difficulty)
    {
        this.db = db;
        this.difficulty = difficulty;
    }
    public int Compare(cHeader x, cHeader y)
    {
        var rankX = db.GetBestRank(x.sid, difficulty);
        var rankY = db.GetBestRank(y.sid, difficulty);
        int resultX = rankX?.Score ?? 0, resultY = rankY?.Score ?? 0;
        int dif = resultY - resultX;
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
    [Inject]
    private IDatabaseAPI db;
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
                return new ChartScoreSort(db, (Difficulty)cl_lastdiff);
            default:
                return new SongNameSort(dataLoader);
        }
    }
}