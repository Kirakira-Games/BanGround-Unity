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
    public static readonly string ChartDir = DataDir + "chart/";
    public static readonly string MusicDir = DataDir + "music/";
    public static readonly string SkinDir = DataDir + "skin/";
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

    private static readonly string TempDir = Application.persistentDataPath + "/temp/";

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
        if (!Directory.Exists(ChartDir))
        {
            Directory.CreateDirectory(ChartDir);
        }
        if (!Directory.Exists(MusicDir))
        {
            Directory.CreateDirectory(MusicDir);
        }

        LiveSetting.Load();

        // Check first launch after updating initial charts
        if (!File.Exists(SongListPath) || PlayerPrefs.GetInt("InitialChartVersion") != InitialChartVersion)
        {
            Debug.Log("Load initial charts...");
            yield return CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");
            LoadKiraPack(new FileInfo(Application.persistentDataPath + "/Initial.kirapack"));
            PlayerPrefs.SetInt("InitialChartVersion", InitialChartVersion);
            File.Delete(Application.persistentDataPath + "/Initial.kirapack");
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
        return File.Exists(GetMusicPath(mid));
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
        }
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
        var newSongList = new SongList();
        Dictionary<string, List<string>> referencedSongs = new Dictionary<string, List<string>>();

        // Scan charts
        DirectoryInfo chartDirectory = new DirectoryInfo(ChartDir);
        DirectoryInfo[] charts = chartDirectory.GetDirectories();

        // First pass
        foreach (var chart in charts)
        {
            string headerPath = chart.FullName + "/cheader.bin";
            if (!File.Exists(headerPath))
            {
                Debug.LogWarning("Missing chart header: " + headerPath);
                continue;
            }
            cHeader chartHeader = ProtobufHelper.Load<cHeader>(headerPath);
            // Update reference
            string mid = chartHeader.mid.ToString();
            if (!referencedSongs.ContainsKey(mid))
            {
                referencedSongs[mid] = new List<string>();
            }
            referencedSongs[mid].Add(chart.Name);
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
            if (!referencedSongs.ContainsKey(song.Name))
            {
                Debug.LogWarning(string.Format("Song {0} is not referenced anymore. GC!", song.Name));
                Directory.Delete(song.FullName, true);
                continue;
            }
            referencedSongs.Remove(song.Name);
            string headerPath = song.FullName + "/mheader.bin";
            if (!File.Exists(headerPath))
            {
                Debug.LogWarning("Missing song header: " + headerPath);
                continue;
            }
            mHeader musicHeader = ProtobufHelper.Load<mHeader>(headerPath);
            newSongList.mHeaders.Add(musicHeader);
        }

        // Second Pass
        foreach (var song in referencedSongs)
        {
            foreach (var sid in song.Value)
            {
                Debug.LogWarning(string.Format("Chart {0} does not have corresponding song {1}. GC!", sid, song.Key));
                Directory.Delete(ChartDir + sid + "/", true);
                newSongList.cHeaders.Remove(newSongList.cHeaders.Find(header => header.sid.ToString() == sid));
            }
        }

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

    private static int ConvertBinAndCopy(string path, string dest)
    {
        if (!Directory.Exists(path)) return -1;
        int ret = -1;
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
            if (int.TryParse(id, out int result))
            {
                ret = result;
            }
            if (!Directory.Exists(dest + id))
            {
                Directory.CreateDirectory(dest + id);
            }
            foreach (var file in files)
            {
                File.Copy(file.FullName, dest + id + "/" + file.Name, true);
            }
        }
        return ret;
    }

    public static int LoadKiraPack(FileInfo file)
    {
        string path = file.FullName;
        if (!File.Exists(path)) return -1;
        Debug.Log("Load kirapack: " + path);
        try
        {
            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                if (Directory.Exists(TempDir))
                {
                    Directory.Delete(TempDir, true);
                }
                zip.ExtractToDirectory(TempDir);
            }

            // Load charts
            int ret = ConvertBinAndCopy(TempDir + "chart/", ChartDir);
            // Load music
            ConvertBinAndCopy(TempDir + "music/", MusicDir);
            //Load Skin
            CopyFolder(TempDir + "skin", SkinDir);
            Directory.Delete(TempDir, true);
            return ret;
        }
        catch (System.Exception e)
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR,
                string.Format("Cannot Load {0}: {1}", file.Name, e.Message));
            return -1;
        }
    }

    public static bool LoadAllKiraPackFromInbox()
    {
        bool LoadSuccess = false;
        if (TitleLoader.IsAprilFool && !Directory.Exists($"{Application.persistentDataPath}/data/chart/233333"))
            SelectManager.letTheBassKick = true;

        try
        {
            if (!Directory.Exists(InboxDir))
            {
                Debug.LogWarning("Inbox directory does not exist.");
            }
            else
            {
                DirectoryInfo packDir = new DirectoryInfo(InboxDir);
                FileInfo[] files = packDir.GetFiles("*.kirapack", SearchOption.TopDirectoryOnly);

                //iOS额外搜索沙盒根目录文件 临时解决导入问题
                if (Application.platform == RuntimePlatform.IPhonePlayer)
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
                        File.Delete(file.FullName);
                    //}
                }
            }
        }
        catch (System.Exception e)
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

    private static void CopyFolder(string src, string des)
    {
        if (!Directory.Exists(src)) return;
        DirectoryInfo dir = new DirectoryInfo(src);
        DirectoryInfo[] subdirs = dir.GetDirectories();
        foreach (var cdir in subdirs)
        {
            FileInfo[] files = cdir.GetFiles();
            string id = cdir.Name;
            if (!Directory.Exists(des + id))
            {
                Directory.CreateDirectory(des + id);
            }
            foreach (var file in files)
            {
                File.Copy(file.FullName, des + id + "/" + file.Name, true);
            }
        }
    }

}
