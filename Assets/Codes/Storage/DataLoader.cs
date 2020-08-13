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
using UnityEngine.SceneManagement;
using UniRx.Async;
using Zenject;
using UnityEngine.Events;

public class DataLoader : IDataLoader
{
    public static IDataLoader Instance; // Temporary singleton for refactoring. TODO: Remove.
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
    public static readonly string ChartDir = "chart/";
    public static readonly string MusicDir = "music/";
    private static readonly string SkinDir = "skin/";
    private static readonly string KirapackDir = Application.persistentDataPath + "/kirapack/";
    private static readonly string FSDir = DataDir + "filesystem/";
    private static readonly string FSIndex = DataDir + "filesystem/fsindex.bin";
    public static readonly string InboxDir = Application.persistentDataPath + "/Inbox/";
    private static readonly string SongListPath = DataDir + "songlist.bin";
    public static readonly string ScoresPath = Application.persistentDataPath + "/Scores.bin";
    public int LastImportedSid { get; set; } = -1;

    [Inject]
    private IChartVersion chartVersion;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;
    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;

    private SongList songList;
    public UnityEvent onSongListRefreshed { get; } = new UnityEvent();
    public List<mHeader> musicList => songList.mHeaders;
    public List<cHeader> chartList => songList.cHeaders;
    public bool loaded => songList != null;

    private const int InitialChartVersion = 6;
    private const int GameVersion = 4;

    private Dictionary<int, cHeader> chartDic = new Dictionary<int, cHeader>();
    private Dictionary<int, mHeader> musicDic = new Dictionary<int, mHeader>();

    public async UniTaskVoid Init()
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
            await CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");
            LoadKiraPack(new FileInfo(Application.persistentDataPath + "/Initial.kirapack"));
            PlayerPrefs.SetInt("InitialChartVersion", InitialChartVersion);
        }

        // Register deep link
        Application.deepLinkActivated += (url) =>
        {
            if (LoadAllKiraPackFromInbox())
            {
                if (SceneManager.GetActiveScene().name == "Select")
                {
                    SceneManager.LoadScene("Select");
                }
            }
        };
    }

    public void InitFileSystem()
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

    public string GetMusicPath(int mid)
    {
        return MusicDir + mid + "/" + mid + ".ogg";
    }

    public bool MusicExists(int mid)
    {
        return KiraFilesystem.Instance.Exists(GetMusicPath(mid));
    }

    public string GetChartResource(int sid, string filename)
    {
        return ChartDir + sid + "/" + filename;
    }

    public string GetChartPath(int sid, Difficulty difficulty)
    {
        return ChartDir + sid + "/" + difficulty.ToString("G").ToLower() + ".bin";
    }

    public string GetChartScriptPath(int sid, Difficulty difficulty)
    {
        return GetChartPath(sid, difficulty).Replace(".bin", ".lua");
    }

    public cHeader GetChartHeader(int sid)
    {
        return chartDic.ContainsKey(sid) ? chartDic[sid] : null;
    }

    [Inject(Id = "r_usevideo")]
    KVar r_usevideo;

    public (string, int) GetBackgroundPath(int sid, bool forceImg = true)
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

    public int GetMidBySid(int sid)
    {
        var header = GetChartHeader(sid);
        if (header != null)
        {
            return header.mid;
        }
        return -1;
    }

    public mHeader GetMusicHeader(int mid)
    {
        return musicDic.ContainsKey(mid) ? musicDic[mid] : null;
    }

    public T LoadChart<T>(int sid, Difficulty difficulty) where T : IExtensible
    {
        return ProtobufHelper.LoadFromKiraFs<T>(GetChartPath(sid, difficulty));
    }

    public void SaveChart<T>(T chart, int sid, Difficulty difficulty) where T : IExtensible
    {
        string path = Path.Combine(DataDir, GetChartPath(sid, difficulty));
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(chart, path);
    }

    public void SaveChartScript(string script, int sid, Difficulty difficulty)
    {
        string path = Path.Combine(DataDir, GetChartScriptPath(sid, difficulty));
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, script);
    }

    public void SaveHeader(cHeader header)
    {
        string path = Path.Combine(DataDir, ChartDir, header.sid.ToString(), "cheader.bin");
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(header, path);
    }

    public void SaveHeader(mHeader header, byte[] oggFile)
    {
        SaveHeader(header);

        string path = Path.Combine(DataDir, MusicDir, header.mid.ToString(), $"{header.mid}.ogg");
        File.WriteAllBytes(path, oggFile);
    }

    public void SaveHeader(mHeader header)
    {
        string path = Path.Combine(DataDir, MusicDir, header.mid.ToString(), "mheader.bin");
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        ProtobufHelper.Save(header, path);
    }

    private void ExtractRelatedFiles(cHeader header, DirectoryInfo dir)
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

    public string BuildKiraPack(cHeader header)
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

    public int GenerateSid()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
    }

    public int GenerateMid()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
    }

    public void DuplicateKiraPack(cHeader header)
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

    public int GetChartLevel(string path)
    {
        try
        {
            try
            {
                var V2chart = ProtobufHelper.LoadFromKiraFs<V2.Chart>(path);
                if (chartVersion.CanRead(V2chart.version))
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
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Update the song list by iterating over all header files.
    /// </summary>
    public void RefreshSongList()
    {
        chartDic.Clear();
        musicDic.Clear();
        songList = new SongList();

        var referencedSongs = new Dictionary<mHeader, int>();
        var loadedIds = new HashSet<int>();

        var files = KiraFilesystem.Instance.ListFiles();

        var musics = from x in files
                     where x.EndsWith("mheader.bin")
                     select x;

        foreach (var music in musics)
        {
            mHeader musicHeader = ProtobufHelper.LoadFromKiraFs<mHeader>(music);
            if (musicHeader.BPM == null || musicHeader.BPM.Length == 0)
                musicHeader.BPM = new float[] { 120, 120 };
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

        foreach (var chart in charts)
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

            songList.cHeaders.Add(chartHeader);
        }

        foreach (var (song, refcount) in referencedSongs)
        {
            if (refcount == 0)
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
                songList.mHeaders.Add(song);
            }
        }

        Debug.Log(JsonConvert.SerializeObject(songList, Formatting.Indented));
        // ProtobufHelper.Save(songList, SongListPath);

        // Populate dic data
        foreach (var music in songList.mHeaders)
            musicDic[music.mid] = music;

        foreach (var chart in songList.cHeaders)
            chartDic[chart.sid] = chart;

        if (cl_lastdiff >= songList.cHeaders.Count)
        {
            cl_lastdiff.Set(Mathf.Max(0, songList.cHeaders.Count - 1));
        }

        onSongListRefreshed.Invoke();
    }

    public void ConvertJsonToBin(DirectoryInfo dir)
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

                    // v2!
                    if (json.Contains("\"version\""))
                    {
                        var chart = JsonConvert.DeserializeObject<V2.Chart>(json);
                        ProtobufHelper.Save(chart, des + ".bin");
                    }
                    else
                    {
                        var chart = JsonConvert.DeserializeObject<Chart>(json);
                        ProtobufHelper.Save(chart, des + ".bin");
                    }
                }
                File.Delete(file.FullName);
            }
        }
    }

    private void ConvertBin(string kirapack)
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
                        // v2!
                        if (json.Contains("\"version\""))
                        {
                            obj = JsonConvert.DeserializeObject<V2.Chart>(json);
                        }
                        else
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

    public int LoadKiraPack(FileInfo file)
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
            MessageBannerController.ShowMsg(LogLevel.ERROR,
                string.Format("Cannot Load {0}: {1}", file.Name, e.Message));
            return -1;
        }
    }

    public bool LoadAllKiraPackFromInbox()
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
                    cl_lastsid.Set(tmp);
                    LoadSuccess = true;
                    MessageBannerController.ShowMsg(LogLevel.OK, "Loaded kirapack: ".GetLocalized() + file.Name);
                }
                //File.Delete(file.FullName);
                //}
            }
            //}
        }
        catch (Exception e)
        {
            MessageBannerController.ShowMsg(LogLevel.ERROR, e.Message, false);
        }
        return LoadSuccess;
    }

    private async UniTaskVoid CopyFileFromStreamingAssetsToPersistentDataPath(string relativePath)
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

        using (UnityWebRequest webRequest = UnityWebRequest.Get(streamingPath))
        {
            await webRequest.SendWebRequest();
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
