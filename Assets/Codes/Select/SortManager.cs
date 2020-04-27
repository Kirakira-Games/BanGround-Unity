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
        int difX = x.difficultyLevel[LiveSetting.actualDifficulty];
        int difY = y.difficultyLevel[LiveSetting.actualDifficulty];
        int dif = difX - difY;
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class SongNameSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = DataLoader.GetMusicHeader(x.mid);
        mHeader ym = DataLoader.GetMusicHeader(y.mid);
        int dif = string.Compare(xm.title, ym.title);
        return dif == 0 ? x.mid - y.mid : dif;
    }
}

public class SongArtistSort : IComparer<cHeader>
{
    public int Compare(cHeader x, cHeader y)
    {
        mHeader xm = DataLoader.GetMusicHeader(x.mid);
        mHeader ym = DataLoader.GetMusicHeader(y.mid);
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
