using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DataLoader
{
#if !UNITY_EDITOR
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
#else 
    public static readonly string DataDir = Application.streamingAssetsPath + "/data/";
#endif
    public static readonly string ChartDir = DataDir + "chart/";
    public static readonly string MusicDir = DataDir + "music/";
    public static readonly string SongListPath = DataDir + "songlist.bin";

    public static SongList songList;
    public static List<mHeader> musicList => songList.mHeaders;
    public static List<cHeader> chartList => songList.cHeaders;

    public const int ChartVersion = 1;

    private static Dictionary<int, cHeader> chartDic;
    private static Dictionary<int, mHeader> musicDic;

    public static void Init()
    {
        RefreshSongList();
        ReloadSongList();
    }

    public static string GetMusicPath(int mid)
    {
        return MusicDir + mid + "/bgm.ogg";
    }

    public static string GetChartPath(int sid, Difficulty difficulty)
    {
        return ChartDir + sid + "/" + difficulty.ToString("G").ToLower() + ".bin";
    }

    public static cHeader GetChartHeader(int sid)
    {
        return chartDic.ContainsKey(sid) ? chartDic[sid] : null;
    }

    public static string GetBackgroundPath(int sid)
    {
        var header = GetChartHeader(sid);
        if (header != null)
        {
            var name = header.backgroundFile.pic ?? header.backgroundFile.pic;
            if (name == null || name.Length == 0)
            {
                return null;
            }
            return ChartDir + sid + "/" + name;
        }
        return null;
    }

    public static int GetMidBySid(int sid)
    {
        var header = GetChartHeader(sid);
        if (header != null)
        {
            return header.mid;
        }
        return -1;
    }

    public static mHeader GetMusicHeader(int mid)
    {
        return musicDic.ContainsKey(mid) ? musicDic[mid] : null;
    }

    public static Chart LoadChart(int sid, Difficulty difficulty)
    {
        return ProtobufHelper.Load<Chart>(GetChartPath(sid, difficulty));
    }

    public static void ReloadSongList()
    {
        chartDic = new Dictionary<int, cHeader>();
        musicDic = new Dictionary<int, mHeader>();
        songList = ProtobufHelper.Load<SongList>(SongListPath);
        foreach (var music in songList.mHeaders)
        {
            musicDic[music.mid] = music;
        }
        foreach (var chart in songList.cHeaders)
        {
            chartDic[chart.sid] = chart;
            // Sanity check
            if (!musicDic.ContainsKey(chart.mid))
            {
                Debug.LogWarning(string.Format("Chart {0} does not have corresponding music {1}.", chart.sid, chart.mid));
            }
        }
    }

    /// <summary>
    /// Update songlist.bin. This will NOT update song list in game!
    /// You probably want to call ReloadSongList afterwards.
    /// </summary>
    public static void RefreshSongList()
    {
        var newSongList = new SongList();

        // Scan charts
        DirectoryInfo chartDirectory = new DirectoryInfo(ChartDir);
        DirectoryInfo[] charts = chartDirectory.GetDirectories();
        foreach (var chart in charts)
        {
            string headerPath = chart.FullName + "/cheader.bin";
            if (!File.Exists(headerPath))
            {
                Debug.LogWarning("Missing chart header: " + headerPath);
                continue;
            }
            cHeader chartHeader = ProtobufHelper.Load<cHeader>(headerPath);
            // Update difficulty
            chartHeader.difficultyLevel = new List<int>();
            for (int diff = 0; diff <= (int)Difficulty.Special; diff++)
            {
                var path = GetChartPath(chartHeader.sid, (Difficulty)diff);
                if (File.Exists(path))
                {
                    chartHeader.difficultyLevel.Add(ProtobufHelper.Load<Chart>(path).level);
                }
                else
                {
                    chartHeader.difficultyLevel.Add(-1);
                }
            }
            newSongList.cHeaders.Add(chartHeader);
        }

        // Scan music
        DirectoryInfo musicDirectory = new DirectoryInfo(MusicDir);
        DirectoryInfo[] songs = musicDirectory.GetDirectories();
        foreach (var song in songs)
        {
            string headerPath = song.FullName + "/mheader.bin";
            if (!File.Exists(headerPath))
            {
                Debug.LogWarning("Missing song header: " + headerPath);
                continue;
            }
            mHeader musicHeader = ProtobufHelper.Load<mHeader>(headerPath);
            newSongList.mHeaders.Add(musicHeader);
        }

        // Save
        Debug.Log(JsonConvert.SerializeObject(newSongList));
        Debug.Log(Application.persistentDataPath);
        ProtobufHelper.Save(newSongList, SongListPath);
    }
}
