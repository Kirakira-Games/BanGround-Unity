using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class DataLoader
{
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
    public static readonly string ChartDir = DataDir + "chart/";
    public static readonly string MusicDir = DataDir + "music/";
    public static readonly string SongListPath = DataDir + "songlist.bin";

    public static SongList songList;
    public static List<mHeader> musicList => songList.mHeaders;
    public static List<cHeader> chartList => songList.cHeaders;

    public const int ChartVersion = 1;
    private const int InitialChartVersion = 3;
    private const int GameVersion = 2;

    private static Dictionary<int, cHeader> chartDic;
    private static Dictionary<int, mHeader> musicDic;

    private static readonly string TempDir = Application.persistentDataPath + "/temp/";
    private static readonly string InboxDir = Application.persistentDataPath + "/Inbox/";

    public static IEnumerator Init()
    {
        // Delete save files of old versions
        if (PlayerPrefs.GetInt("GameVersion") == 0)
        {
            Debug.Log("Remove old save files.");
            var files = new DirectoryInfo(Application.persistentDataPath).GetFiles();
            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
            PlayerPrefs.SetInt("GameVersion", GameVersion);
        }
        
        // Create directories
        if (!Directory.Exists(ChartDir))
        {
            Directory.CreateDirectory(ChartDir);
        }
        if (!Directory.Exists(MusicDir))
        {
            Directory.CreateDirectory(MusicDir);
        }
        //// Check first launch after updating initial charts
        //if (!File.Exists(SongListPath) || PlayerPrefs.GetInt("InitialChartVersion") != InitialChartVersion)
        //{
        //    Debug.Log("Load initial charts...");
        //    yield return CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");
        //    LoadKiraPack(Application.persistentDataPath + "/Initial.kirapack");
        //    PlayerPrefs.SetInt("InitialChartVersion", InitialChartVersion);
        //    File.Delete(Application.persistentDataPath + "/Initial.kirapack");
        //}

        yield return new WaitForEndOfFrame();

        LiveSetting.Load();
//#if UNITY_ANDROID && !UNITY_EDITOR
//        if (LiveSetting.bufferSize == -1)
//            LiveSetting.bufferSize = AppPreLoader.bufferSize;
//#endif
    }

    public static string GetMusicPath(int mid)
    {
        return MusicDir + mid + "/" + mid + ".ogg";
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

        // Sort And Save
        newSongList.cHeaders.Sort(new cHeaderComparer());
        Debug.Log(JsonConvert.SerializeObject(newSongList));
        ProtobufHelper.Save(newSongList, SongListPath);
    }

    public static void ConvertJsonToBin(DirectoryInfo dir)
    {
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".json")
            {
                string json = File.ReadAllText(file.FullName);

                if (file.Name == "cheader.json")
                {
                    cHeader header = JsonConvert.DeserializeObject<cHeader>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(header, des + ".bin");
                }
                else if (file.Name == "mheader.json")
                {
                    mHeader header = JsonConvert.DeserializeObject<mHeader>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(header, des + ".bin");
                }
                else
                {
                    Chart chart = JsonConvert.DeserializeObject<Chart>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(chart, des + ".bin");
                }
                File.Delete(file.FullName);
            }
        }
    }

    private static void ConvertBinAndCopy(string path, string dest)
    {
        if (!Directory.Exists(path)) return;
        DirectoryInfo dir = new DirectoryInfo(path);
        DirectoryInfo[] subdirs = dir.GetDirectories();
        foreach (var cdir in subdirs)
        {
            ConvertJsonToBin(cdir);
        }
        foreach (var cdir in subdirs)
        {
            FileInfo[] files = cdir.GetFiles();
            string id = cdir.Name;
            if (!Directory.Exists(dest + id))
            {
                Directory.CreateDirectory(dest + id);
            }
            foreach (var file in files)
            {
                File.Copy(file.FullName, dest + id + "/" + file.Name, true);
            }
        }
    }

    public static void LoadKiraPack(string path)
    {
        if (!File.Exists(path)) return;
        Debug.Log("Load kirapack: " + path);
        using (ZipArchive zip = ZipFile.OpenRead(path))
        {
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
            zip.ExtractToDirectory(TempDir);
        }

        // Load charts
        ConvertBinAndCopy(TempDir + "chart/", ChartDir);
        // Load music
        ConvertBinAndCopy(TempDir + "music/", MusicDir);
        Directory.Delete(TempDir, true);
    }

    public static bool LoadAllKiraPackFromInbox()
    {
        bool LoadSuccess = false;
        try
        {
            if (!Directory.Exists(InboxDir))
            {
                Debug.LogWarning("Inbox directory does not exist.");
            }
            else
            {
                DirectoryInfo packDir = new DirectoryInfo(InboxDir);
                FileInfo[] files = packDir.GetFiles();
                foreach (var file in files)
                {
                    if (file.Extension == ".kirapack")
                    {
                        LoadKiraPack(file.FullName);
                        MessageBoxController.ShowMsg(LogLevel.INFO, "Loaded kirapack: ".GetLocalized() + file.Name);
                        File.Delete(file.FullName);
                        LoadSuccess = true;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, e.Message, false);
        }
        if (LoadSuccess)
        {
            RefreshSongList();
            ReloadSongList();
        }
        return LoadSuccess;
    }

    private static IEnumerator CopyFileFromStreamingAssetsToPersistentDataPath(string relativePath)
    {
        string streamingPath;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.Android)  
        {
            streamingPath = Application.streamingAssetsPath + relativePath;
        }
        else
        {
            streamingPath = "file://" + Application.streamingAssetsPath + relativePath;
        }

        Debug.Log(streamingPath);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(streamingPath))
        {
            yield return webRequest.SendWebRequest();
            string directory = Path.GetDirectoryName(Application.persistentDataPath + relativePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            using (var writer = File.Create(Application.persistentDataPath + relativePath))
            {
                writer.Write(webRequest.downloadHandler.data, 0, webRequest.downloadHandler.data.Length);
            }
            Debug.Log($"Copy File {relativePath} {!webRequest.isNetworkError}");
        }
    }

}
