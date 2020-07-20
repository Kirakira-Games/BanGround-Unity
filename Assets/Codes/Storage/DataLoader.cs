using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Linq;
using ProtoBuf;

public static class DataLoader
{
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
    public static readonly string ChartDir = "chart/";
    public static readonly string MusicDir = "music/";
    public static readonly string SkinDir = "skin/";
    public static readonly string KirapackDir = Application.persistentDataPath + "/kirapack/";
    public static readonly string FSDir = DataDir + "filesystem/";
    public static readonly string FSIndex = DataDir + "filesystem/fsindex.bin";
    public static readonly string InboxDir = Application.persistentDataPath + "/Inbox/";
    public static readonly string SongListPath = DataDir + "songlist.bin";
    public static int LastImportedSid = -1;

    public static SongList songList;
    public static List<mHeader> musicList => songList.mHeaders;
    public static List<cHeader> chartList => songList.cHeaders;
    public static bool loaded => songList != null;

    private const int InitialChartVersion = 5;
    private const int GameVersion = 4;

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
                if (file.Name == "LiveSettings.json")
                    File.Delete(file.FullName);
            }
            PlayerPrefs.SetInt("GameVersion", GameVersion);
        }

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

    public static void InitFileSystem()
    {
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

    static KVarRef r_usevideo = new KVarRef("r_usevideo");

    public static (string, int) GetBackgroundPath(int sid, bool forceImg = true)
    {
        var header = GetChartHeader(sid);
        if (header != null)
        {
            var type = 0;

            if (!forceImg && r_usevideo && !string.IsNullOrEmpty(header.backgroundFile.vid))
                type = 1;

            var name = type == 1 ? header.backgroundFile.vid : header.backgroundFile.pic;

            if (name == null || name.Length == 0)
            {
                return (null, 0);
            }

            return (ChartDir + sid + "/" + name, type);
        }
        return (null, 0);
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

    public static T LoadChart<T>(int sid, Difficulty difficulty) where T : IExtensible
    {
        return ProtobufHelper.LoadFromKiraFs<T>(GetChartPath(sid, difficulty));
    }

    public static void SaveChart<T>(T chart, int sid, Difficulty difficulty) where T : IExtensible
    {
        string path = Path.Combine(DataDir, GetChartPath(sid, difficulty));
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(chart, path);
    }

    public static void SaveHeader(cHeader header)
    {
        string path = Path.Combine(DataDir, ChartDir, header.sid.ToString(), "cheader.bin");
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(header, path);
    }

    public static void SaveHeader(mHeader header)
    {
        string path = Path.Combine(DataDir, MusicDir, header.mid.ToString(), "mheader.bin");
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(header, path);
    }

    private static void ExtractRelatedFiles(cHeader header, DirectoryInfo dir)
    {
        if (dir.Exists)
            dir.Delete(true);
        dir.Create();
        // Find possible related files
        var files = KiraFilesystem.Instance.ListFiles((path) =>
        {
            path = path.Replace("\\", "/");
            return path.StartsWith(ChartDir + header.sid + "/") ||
                path.StartsWith(MusicDir + header.mid + "/");
        });
        foreach (var file in files)
        {
            var dirpath = Path.Combine(dir.FullName, Path.GetDirectoryName(file));
            if (!Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);
            File.WriteAllBytes(Path.Combine(dir.FullName, file), KiraFilesystem.Instance.Read(file));
        }
    }

    public static string BuildKiraPack(cHeader header)
    {
        var dir = new DirectoryInfo(Path.Combine(KirapackDir, "temp/"));
        ExtractRelatedFiles(header, dir);
        var zippath = Path.Combine(KirapackDir, header.sid + ".kirapack");
        if (File.Exists(zippath))
            File.Delete(zippath);
        ZipFile.CreateFromDirectory(dir.FullName, zippath);
        dir.Delete(true);
        return zippath;
    }

    public static int GenerateSid()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (int) (DateTime.UtcNow - epochStart).TotalSeconds;
    }

    public static void DuplicateKiraPack(cHeader header)
    {
        var dir = new DirectoryInfo(Path.Combine(KirapackDir, "temp/"));
        ExtractRelatedFiles(header, dir);

        // Move directory
        int newsid = GenerateSid();
        var newdir = Path.Combine(dir.FullName, "chart/", newsid + "/");
        Directory.Move(Path.Combine(dir.FullName, "chart/", header.sid + "/"), newdir);

        // Save new header
        header.sid = newsid;
        ProtobufHelper.Save(header, Path.Combine(newdir, "cheader.bin"));

        // Add to zip file
        var zippath = Path.Combine(Application.persistentDataPath, newsid + ".kirapack");
        if (File.Exists(zippath))
            File.Delete(zippath);
        ZipFile.CreateFromDirectory(dir.FullName, zippath);
        dir.Delete(true);
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

    public static int GetChartLevel(string path)
    {
        try
        {
            var V2chart = ProtobufHelper.LoadFromKiraFs<V2.Chart>(path);
            if (ChartVersion.CanRead(V2chart.version))
            {
                return V2chart.level;
            }
            throw new InvalidDataException("Failed to read with V2 format, fallback to old chart.");
        }
        catch
        {
            return ProtobufHelper.LoadFromKiraFs<Chart>(path).level;
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
        var referencedSongs = new Dictionary<mHeader, int>();
        var loadedIds = new HashSet<int>();

        var files = KiraFilesystem.Instance.ListFiles();

        var musics = from x in files
                     where x.EndsWith("mheader.bin")
                     select x;

        foreach (var music in musics)
        {
            mHeader musicHeader = ProtobufHelper.LoadFromKiraFs<mHeader>(music);
            if (!loadedIds.Contains(musicHeader.mid))
            {
                referencedSongs.Add(musicHeader, 0);
                loadedIds.Add(musicHeader.mid);
            }
        }

        loadedIds.Clear();
        var charts = from x in files
                     where x.EndsWith("cheader.bin")
                     select x;

        foreach(var chart in charts)
        {
            cHeader chartHeader = ProtobufHelper.LoadFromKiraFs<cHeader>(chart);
            if (loadedIds.Contains(chartHeader.sid))
                continue;
            loadedIds.Add(chartHeader.sid);
            // Update reference
            var mid = chartHeader.mid;

            try
            {
                var music = referencedSongs.First(mc => mc.Key.mid == mid);
                referencedSongs[music.Key]++;
            }
            catch
            {
                Debug.LogWarning($"Chart {chartHeader.sid} does not have corresponding song {chartHeader.mid}. GC!");
                continue;
            }

            chartHeader.difficultyLevel = new List<int>();

            for (var diff = Difficulty.Easy; diff <= Difficulty.Special; diff++)
            {
                var path = chart.Replace("cheader.bin", $"{diff.ToString().ToLower()}.bin");
                if (files.Contains(path))
                {
                    chartHeader.difficultyLevel.Add(GetChartLevel(path));
                }
                else
                {
                    chartHeader.difficultyLevel.Add(-1);
                }
            }

            newSongList.cHeaders.Add(chartHeader);
        }

        

        foreach (var (song, refcount) in referencedSongs)
        {
            if(refcount == 0)
            {
                var musicDir = $"music/{song.mid}";

                var musicFiles = KiraFilesystem.Instance.ListFiles(name => name.Contains(musicDir));
                musicFiles.All(item =>
                {
                    KiraFilesystem.Instance.RemoveFileFromIndex(item);
                    return true;
                });

                KiraFilesystem.Instance.SaveIndex();

                Debug.Log($"Removing music {song} due to the refcount is zero");
            }
            else
            {
                newSongList.mHeaders.Add(song);
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
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    try
                    {
                        var chart = JsonConvert.DeserializeObject<V2.Chart>(json);
                        if (ChartVersion.CanRead(chart.version))
                        {
                            ProtobufHelper.Save(chart, des + ".bin");
                            return;
                        }
                        throw new InvalidDataException("Failed to read with V2 format, fallback to old chart.");
                    }
                    catch
                    {
                        var chart = JsonConvert.DeserializeObject<Chart>(json);
                        ProtobufHelper.Save(chart, des + ".bin");
                    }
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

                Type type = null;

                if (entry.Name == "cheader.json")
                    type = typeof(cHeader);
                else if (entry.Name == "mheader.json")
                    type = typeof(mHeader);

                using (var sr = new StreamReader(entry.Open()))
                {
                    var json = sr.ReadToEnd();
                    object obj;
                    if (type == null)
                    {
                        try
                        {
                            var chart = JsonConvert.DeserializeObject<V2.Chart>(json);
                            if (!ChartVersion.CanRead(chart.version))
                            {
                                throw new InvalidDataException("Failed to read with V2 format, fallback to old chart.");
                            }
                            obj = chart;
                        }
                        catch
                        {
                            obj = JsonConvert.DeserializeObject<Chart>(json);
                        }
                    }
                    else
                    {
                        obj = JsonConvert.DeserializeObject(json, type);
                    }

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
