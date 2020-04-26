using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Assertions.Must;

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
    public int Compare(cHeader x, cHeader y)
    {
        return x.difficultyLevel[LiveSetting.actualDifficulty] - y.difficultyLevel[LiveSetting.actualDifficulty];
    }
}

public class SongNameSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = DataLoader.GetMusicHeader(x.mid);
        mHeader ym = DataLoader.GetMusicHeader(y.mid);
        return string.Compare(xm.title, ym.title);
    }
}

public class SongArtistSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = DataLoader.GetMusicHeader(x.mid);
        mHeader ym = DataLoader.GetMusicHeader(y.mid);
        return string.Compare(xm.artist, ym.artist);
    }
}

public class ChartAuthorSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        string xauthor = string.IsNullOrWhiteSpace(x.authorNick) ? x.author : x.authorNick;
        string yauthor = string.IsNullOrWhiteSpace(y.authorNick) ? y.author : y.authorNick;
        return string.Compare(xauthor, yauthor);
    }
}

public class ChartScoreSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        IEnumerable<PlayResult> ListX = PlayRecords.OpenRecord().resultsList.Where(o => o.ChartId == x.sid);
        IEnumerable<PlayResult> ListY = PlayRecords.OpenRecord().resultsList.Where(o => o.ChartId == y.sid);
        double resultX = 0, resultY = 0;
        if (ListX.Count() > 0) resultX = ListX.Max(o => o.Score);
        if (ListY.Count() > 0) resultY = ListY.Max(o => o.Score);
        return (int)(resultX - resultY);
    }
}