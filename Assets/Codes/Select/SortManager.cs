using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Sorter
{
    ChartDifficulty,
    SongName,
    SongArtist,
    ChartAuthor
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