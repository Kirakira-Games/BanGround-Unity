using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Linq;

public class DataLoader
{
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
    public static readonly string ChartDir = "chart/";
    public static readonly string MusicDir = "music/";
    public static readonly string SkinDir = "skin/";
    public static readonly string FSDir = DataDir + "filesystem/";
    public static readonly string FSIndex = DataDir + "filesystem/fsindex.bin";
    public static readonly string InboxDir = Application.persistentDataPath + "/Inbox/";
    public static readonly string SongListPath = DataDir + "songlist.bin";
    public static int LastImportedSid = -1;

    public static SongList songList;
    public static List<mHeader> musicList => songList.mHeaders;
    public static List<cHeader> chartList => songList.cHeaders;

    public const int ChartVersion = 1;
    private const int InitialChartVersion = 4;
    private const int GameVersion = 3;

    private static Dictionary<int, cHeader> chartDic;
    private static Dictionary<int, mHeader> musicDic;

    public static IEnumerator Init()
    {
        LastImportedSid = -1;
        // Delete save files of old versions
        if (PlayerPrefs.GetInt("GameVersion") != GameVersion)
        {
            Debug.Log("Remove old save files.");
            var files = new DirectoryInfo(Application.persistentDataPath).GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if(file.Name != "Player.log")
                File.Delete(file.FullName);
            }
            PlayerPrefs.SetInt("GameVersion", GameVersion);
        }
        
        // Create directories
        if (!Directory.Exists(Path.Combine(DataDir, ChartDir)))
        {
            Directory.CreateDirectory(Path.Combine(DataDir, ChartDir));
        }
        if (!Directory.Exists(Path.Combine(DataDir, MusicDir)))
        {
            Directory.CreateDirectory(Path.Combine(DataDir, MusicDir));
        }
        if (!Directory.Exists(FSDir))
        {
            Directory.CreateDirectory(FSDir);
        }

        new KiraFilesystem(FSIndex, DataDir);

        LiveSetting.Load();

        // Check first launch after updating initial charts
        if (!File.Exists(SongListPath) || PlayerPrefs.GetInt("InitialChartVersion") != InitialChartVersion)
        {
            Debug.Log("Load initial charts...");
            yield return CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");
            LoadKiraPack(new FileInfo(Application.persistentDataPath + "/Initial.kirapack"));
            PlayerPrefs.SetInt("InitialChartVersion", InitialChartVersion);
        }

#if UNITY_ANDROID && false
        AndroidCallback.Init();
#endif

    }

    public static string GetMusicPath(int mid)
    {
        return MusicDir + mid + "/" + mid + ".ogg";
    }

    public static bool MusicExists(int mid)
    {
        return KiraFilesystem.Instance.Exists(GetMusicPath(mid));
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
        return ProtobufHelper.LoadFromKiraFs<Chart>(GetChartPath(sid, difficulty));
    }

    public static void ReloadSongList()
    {
        songList = ProtobufHelper.Load<SongList>(SongListPath);

        foreach (var music in songList.mHeaders)
            musicDic[music.mid] = music;
        
        foreach (var chart in songList.cHeaders)
            chartDic[chart.sid] = chart;
        
        if (LiveSetting.currentChart >= songList.cHeaders.Count)
        {
            LiveSetting.currentChart = Mathf.Max(0, songList.cHeaders.Count - 1);
        }
    }

    /// <summary>
    /// Update songlist.bin. This will NOT update song list in game!
    /// You probably want to call ReloadSongList afterwards.
    /// </summary>
    public static void RefreshSongList()
    {
        chartDic = new Dictionary<int, cHeader>();
        musicDic = new Dictionary<int, mHeader>();

        var newSongList = new SongList();
        var referencedSongs = new Dictionary<string, List<string>>();

        var files = KiraFilesystem.Instance.ListFiles();

        var charts = from x in files
                     where x.EndsWith("cheader.bin")
                     select x;

        foreach(var chart in charts)
        {
            cHeader chartHeader = ProtobufHelper.LoadFromKiraFs<cHeader>(chart);
            // Update reference
            string mid = chartHeader.mid.ToString();

            if (!referencedSongs.ContainsKey(mid))
            {
                referencedSongs[mid] = new List<string>();
            }

            referencedSongs[mid].Add(chart);

            chartHeader.difficultyLevel = new List<int>();

            for (var diff = Difficulty.Easy; diff <= Difficulty.Special; diff++)
            {
                var path = chart.Replace("cheader.bin", $"{diff.ToString().ToLower()}.bin");
                if (files.Contains(path))
                {
                    chartHeader.difficultyLevel.Add(ProtobufHelper.LoadFromKiraFs<Chart>(path).level);
                }
                else
                {
                    chartHeader.difficultyLevel.Add(-1);
                }
            }

            newSongList.cHeaders.Add(chartHeader);
        }

        var musics = from x in files
                     where x.EndsWith("mheader.bin")
                     select x;

        foreach (var music in musics)
        {
            mHeader musicHeader = ProtobufHelper.LoadFromKiraFs<mHeader>(music);
            referencedSongs.Remove(musicHeader.mid.ToString());
            
            newSongList.mHeaders.Add(musicHeader);
        }

        foreach (var song in referencedSongs)
        {
            foreach (var sid in song.Value)
            {
                Debug.LogWarning(string.Format("Chart {0} does not have corresponding song {1}. GC!", sid, song.Key));
                newSongList.cHeaders.Remove(newSongList.cHeaders.Find(header => header.sid.ToString() == sid));
            }
        }

        Debug.Log(JsonConvert.SerializeObject(newSongList, Formatting.Indented));
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

    private static void ConvertBin(string kirapack)
    {
        using (var zip = ZipFile.OpenRead(kirapack))
        {

            var entries = (from x in zip.Entries
                           where x.FullName.EndsWith(".json")
                           select x).ToArray();

            var binEntries = (from x in zip.Entries
                              where x.FullName.EndsWith(".bin")
                              select x.FullName).ToArray();

            foreach (var entry in entries)
            {
                if (binEntries.Contains(entry.FullName.Replace(".json", ".bin")))
                    continue;

                var type = typeof(Chart);

                if (entry.Name == "cheader.json")
                    type = typeof(cHeader);
                else if (entry.Name == "mheader.json")
                    type = typeof(mHeader);

                using (var sr = new StreamReader(entry.Open()))
                {
                    var json = sr.ReadToEnd();
                    var obj = JsonConvert.DeserializeObject(json, type);

                    var dir = Path.Combine(DataDir, entry.FullName.Replace(entry.Name, ""));
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    ProtobufHelper.Save(obj, Path.Combine(DataDir, entry.FullName.Replace(".json", ".bin")));
                }
            }
        }
    }

    public static int LoadKiraPack(FileInfo file)
    {
        string path = Path.Combine(FSDir, Guid.NewGuid().ToString("N"));

        if (!file.Exists) 
            return -1;

        Debug.Log($"Load kirapack: {file.FullName}");

        try
        {
            File.Move(file.FullName, path);
            KiraFilesystem.Instance.AddToIndex(path);
            KiraFilesystem.Instance.SaveIndex();

            // Load charts
            ConvertBin(path);

            int ret = -1;
            using (var zip = ZipFile.OpenRead(path))
            {
                foreach (var entry in zip.Entries)
                {
                    if (!entry.FullName.Contains("music") && int.TryParse(entry.FullName.Replace("chart/", "").Replace("/" + entry.Name, ""), out ret))
                    {
                        break;
                    }
                }
            }

            return ret;
        }
        catch (Exception e)
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR,
                string.Format("Cannot Load {0}: {1}", file.Name, e.Message));
            return -1;
        }
    }

    public static bool LoadAllKiraPackFromInbox()
    {
        bool LoadSuccess = false;
        if (TitleLoader.IsAprilFool && !KiraFilesystem.Instance.Exists($"data/chart/233333/special.bin"))
            SelectManager.letTheBassKick = true;

        try
        {
            //if (!Directory.Exists(InboxDir))
            //{
            //    Debug.LogWarning("Inbox directory does not exist.");
            //}
            //else
            //{
            DirectoryInfo packDir = new DirectoryInfo(InboxDir);
            FileInfo[] files = new FileInfo[] { };
            if (packDir.Exists)
                files = packDir.GetFiles("*.kirapack", SearchOption.TopDirectoryOnly);

            //iOS额外搜索沙盒根目录文件 临时解决导入问题
            if (Application.platform == RuntimePlatform.IPhonePlayer || true)
            {
                var packDir2 = new DirectoryInfo(Application.persistentDataPath);
                var files2 = packDir2.GetFiles("*.kirapack", SearchOption.TopDirectoryOnly);
                files = files.Concat(files2).ToArray();
            }

            foreach (var file in files)
            {
                //if (file.Extension == ".kirapack")
                //{
                int tmp = LoadKiraPack(file);
                if (tmp != -1)
                {
                    LastImportedSid = tmp;
                    LoadSuccess = true;
                    MessageBoxController.ShowMsg(LogLevel.OK, "Loaded kirapack: ".GetLocalized() + file.Name);
                }
                //File.Delete(file.FullName);
                //}
            }
            //}
        }
        catch (Exception e)
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, e.Message, false);
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
