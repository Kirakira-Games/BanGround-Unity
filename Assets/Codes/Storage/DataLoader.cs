using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Linq;
using ProtoBuf;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine.Events;
using BanGround;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Text;
using BanGround.Database;

public class DataLoader : IDataLoader
{
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
    public static readonly string ChartDir = "chart/";
    public static readonly string ReplayDir = "replay/";
    public static readonly string MusicDir = "music/";
    private static readonly string SkinDir = "skin/";
    private static readonly string KirapackDir = Application.persistentDataPath + "/kirapack/";
    [Obsolete("No longer used as a result of file system migration.")]
    public static readonly string FSDir = DataDir + "filesystem/";
    private static readonly string FSIndex = DataDir + "filesystem/fsindex.bin";
    public static readonly string InboxDir = Application.persistentDataPath + "/Inbox/";
    public int LastImportedSid { get; set; } = -1;

    [Inject]
    LocalizedStrings localizedStrings;
    [Inject]
    private IChartVersion chartVersion;
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private IFileSystem fs;
    [Inject]
    private ILoadingBlocker loadingBlocker;
    [Inject]
    private IDatabaseAPI db;
    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;

    private SongList songList;
    public UnityEvent onSongListRefreshed { get; } = new UnityEvent();
    public List<mHeader> musicList => songList.mHeaders;
    public List<cHeader> chartList => songList.cHeaders;
    public bool loaded => songList != null;

    private const int InitialChartVersion = 7;
    private const int GameVersion = 4;

    private Dictionary<int, cHeader> chartDic = new Dictionary<int, cHeader>();
    private Dictionary<int, mHeader> musicDic = new Dictionary<int, mHeader>();

    private void SanityCheck()
    {
        var referencedSongs = new Dictionary<int, int>();
        // Music files with wrong filename
        Regex musicFileName = new Regex("^" + MusicDir.Replace("/", @"\/") + @"([0-9]+)\/([0-9]+)\..*$");
        var toRemove = new HashSet<int>();
        foreach (var file in fs)
        {
            var filename = file.Name;
            var matches = musicFileName.Matches(filename);
            if (matches.Count == 0)
                continue;
            var groups = matches[0].Groups;
            int id1 = int.Parse(groups[1].Value);
            int id2 = int.Parse(groups[2].Value);
            if (id1 != id2)
            {
                file.Name = filename.Substring(0, groups[2].Index) + id1 + filename.Substring(groups[2].Index + groups[2].Length);
                Debug.Log("Fixed ID: " + file.Name);
            }
        }
        // Fix music headers
        Regex musicHeaderName = new Regex("^" + MusicDir.Replace("/", @"\/") + @"([0-9]+)\/mheader\.bin$");
        foreach (var file in fs)
        {
            var filename = file.Name;
            var matches = musicHeaderName.Matches(filename);
            if (matches.Count == 0)
                continue;
            var groups = matches[0].Groups;
            int id = int.Parse(groups[1].Value);
            mHeader musicHeader = null;
            try
            {
                musicHeader = ProtobufHelper.Load<mHeader>(file);
            }
            catch (Exception e) 
            {
                Debug.LogWarning(e + "\n" + e.StackTrace);
            }
            if (musicHeader == null || !fs.FileExists(GetMusicPath(id)))
            {
                Debug.Log($"Corrupted music header at {id}. GC!");
                toRemove.Add(id);
                continue;
            }
            bool rewrite = false;
            if (musicHeader.mid != id)
            {
                Debug.Log($"Unmatched mid: {musicHeader.mid} in dir {id}. Update header!");
                musicHeader.mid = id;
                rewrite = true;
            }
            if (musicHeader.BPM == null || musicHeader.BPM.Length == 0)
            {
                musicHeader.BPM = new float[] { 120, 120 };
                rewrite = true;
            }
            if (rewrite)
            {
                SaveHeader(musicHeader);
            }
            if (!referencedSongs.ContainsKey(musicHeader.mid))
            {
                referencedSongs.Add(musicHeader.mid, 0);
            }
        }
        foreach (var id in toRemove)
        {
            DeleteMusic(id);
        }
        toRemove.Clear();
        // Fix chart headers
        Regex chartHeaderName = new Regex("^" + ChartDir.Replace("/", @"\/") + @"([0-9]+)\/cheader\.bin$");
        foreach (var file in fs)
        {
            var filename = file.Name;
            var matches = chartHeaderName.Matches(filename);
            if (matches.Count == 0)
                continue;
            var groups = matches[0].Groups;
            int id = int.Parse(groups[1].Value);
            cHeader chartHeader = null;
            try
            {
                chartHeader = ProtobufHelper.Load<cHeader>(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e + "\n" + e.StackTrace);
            }
            if (chartHeader == null)
            {
                Debug.Log($"Corrupted chart header at {id}. GC!");
                toRemove.Add(id);
                continue;
            }
            if (chartHeader.sid != id)
            {
                Debug.Log($"Unmatched sid: {chartHeader.sid} in dir {id}. Update header!");
                chartHeader.sid = id;
                SaveHeader(chartHeader);
            }
            if (referencedSongs.ContainsKey(chartHeader.mid))
            {
                referencedSongs[chartHeader.mid]++;
            }
        }
        foreach (var id in toRemove)
        {
            DeleteChart(id);
        }
        // Remove songs that are not referenced by any chart
        foreach (var (mid, refcount) in referencedSongs)
        {
            if (refcount == 0)
            {
                DeleteMusic(mid);
                Debug.LogWarning($"Removing music {mid} due to zero refcount");
            }
        }
    }

    public async UniTaskVoid Init()
    {
        LastImportedSid = -1;

        // Check first launch after updating initial charts
        RefreshSongList();
        if (chartList.Count == 0 || PlayerPrefs.GetInt("InitialChartVersion") != InitialChartVersion)
        {
            Debug.Log("Load initial charts...");
            await CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");
            //LoadKiraPack(new FileInfo(Application.persistentDataPath + "/Initial.kirapack"));
            PlayerPrefs.SetInt("InitialChartVersion", InitialChartVersion);
        }

        // Register deep link
        Application.deepLinkActivated += (url) =>
        {
            if (SceneManager.GetActiveScene().name == "Select")
            {
                SceneLoader.LoadScene("Select");
            }
        };
    }

    public void InitFileSystem()
    {
        // Create directories
        if (!Directory.Exists(KiraPath.Combine(DataDir, ChartDir)))
        {
            Directory.CreateDirectory(KiraPath.Combine(DataDir, ChartDir));
        }
        if (!Directory.Exists(KiraPath.Combine(DataDir, MusicDir)))
        {
            Directory.CreateDirectory(KiraPath.Combine(DataDir, MusicDir));
        }
        /*
        if (!Directory.Exists(FSDir))
        {
            Directory.CreateDirectory(FSDir);
        }
        */
        if(!Directory.Exists(KiraPath.Combine(DataDir, ReplayDir)))
        {
            Directory.CreateDirectory(KiraPath.Combine(DataDir, ReplayDir));
        }

        if(File.Exists(FSIndex))
        {
            Dictionary<string, string> index;

            using (FileStream fs = File.OpenRead(FSIndex))
            {
                var bf = new BinaryFormatter();
                index = bf.Deserialize(fs) as Dictionary<string, string>;
            }

            List<string> kirapacks = new List<string>();

            foreach(var (_, kirapack) in index)
            {
                if(!kirapacks.Contains(kirapack))
                {
                    if(File.Exists(kirapack))
                    {
                        File.Move(kirapack, kirapack + ".kpak");
                    }
                }
            }

            File.Delete(FSIndex);
        }

        fs.Init();

        //fs.AddSearchPath(FSDir);

        // Sanity check to fix common fs issues
        SanityCheck();
    }

    public string GetMusicResource(int mid, string filename)
    {
        return MusicDir + mid + "/" + filename;
    }

    public string GetMusicPath(int mid)
    {
        return GetMusicResource(mid, mid + ".ogg");
    }
        
    public bool MusicExists(int mid)
    {
        return fs.FileExists(GetMusicPath(mid));
    }

    public string GetChartResource(int sid, string filename)
    {
        return ChartDir + sid + "/" + filename;
    }

    public string GetChartPath(int sid, Difficulty difficulty)
    {
        return GetChartResource(sid, difficulty.Lower() + ".bin");
    }

    public string GetChartScriptPath(int sid, Difficulty difficulty)
    {
        return GetChartResource(sid, difficulty.Lower() + ".lua");
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
        if (header?.backgroundFile != null)
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
        return ProtobufHelper.Load<T>(fs.GetFile(GetChartPath(sid, difficulty)));
    }

    public void SaveChart<T>(T chart, int sid, Difficulty difficulty) where T : IExtensible
    {
        string path = GetChartPath(sid, difficulty);

        var file = fs.GetOrNewFile(path);
        ProtobufHelper.Write(chart, file);
        //fs.FlushPak(file.RootPath);
    }

    public void SaveChartScript(string script, int sid, Difficulty difficulty)
    {
        string path = GetChartScriptPath(sid, difficulty);

        var file = fs.GetOrNewFile(path);
        file.WriteBytes(Encoding.UTF8.GetBytes(script));
        //fs.FlushPak(file.RootPath);
    }

    public void SaveHeader(cHeader header, string coverExt = null, byte[] cover = null)
    {
        string path = KiraPath.Combine(ChartDir, header.sid.ToString(), "cheader.bin");

        var file = fs.GetOrNewFile(path);
        ProtobufHelper.Write(header, file);

        if (cover != null)
        {
            string bgPath = KiraPath.Combine(ChartDir, header.sid.ToString(), "bg" + coverExt);
            var bgFile = fs.GetOrNewFile(bgPath);
            bgFile.WriteBytes(cover);

            //fs.FlushPak(bgFile.RootPath);
        }

        header.LoadDifficultyLevels(this);
        db.SaveChartSet(header.sid, header.mid, header.difficultyLevel.ToArray());
        //fs.FlushPak(file.RootPath);
    }

    public void SaveHeader(mHeader header)
    {
        string path = GetMusicResource(header.mid, "mheader.bin");

        var file = fs.GetOrNewFile(path);
        ProtobufHelper.Write(header, file);
        //fs.FlushPak(file.RootPath);
    }

    public void SaveHeader(mHeader header, byte[] oggFile)
    {
        SaveHeader(header);

        if (oggFile != null)
        {
            string path = GetMusicPath(header.mid);

            var file = fs.GetOrNewFile(path);
            file.WriteBytes(oggFile);
        }
        //fs.FlushPak(file.RootPath);
    }

    private void ExtractRelatedFiles(cHeader header, DirectoryInfo dir)
    {
        if (dir.Exists)
            dir.Delete(true);

        dir.Create();

        // Find possible related files
        var files = fs.ListDirectory(GetChartResource(header.sid, ""))
            .Concat(fs.ListDirectory(GetMusicResource(header.mid, ""))).ToArray();

        foreach (var file in files)
        {
            var dirpath = KiraPath.Combine(dir.FullName, KiraPath.GetDirectoryName(file.Name));
            if (!Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);
            File.WriteAllBytes(KiraPath.Combine(dir.FullName, file.Name), file.ReadToEnd());
        }
    }

    public string BuildKiraPack(cHeader header)
    {
        var dir = new DirectoryInfo(KiraPath.Combine(KirapackDir, "temp/"));
        ExtractRelatedFiles(header, dir);
        var zippath = KiraPath.Combine(KirapackDir, header.sid + ".kirapack");
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
        var dir = new DirectoryInfo(KiraPath.Combine(KirapackDir, "temp/"));
        ExtractRelatedFiles(header, dir);

        // Move directory
        int newsid = GenerateSid();
        var newdir = KiraPath.Combine(dir.FullName, "chart/", newsid + "/");
        Directory.Move(KiraPath.Combine(dir.FullName, "chart/", header.sid + "/"), newdir);

        // Save new header
        header.sid = newsid; 
        ProtobufHelper.Save(header, KiraPath.Combine(newdir, "cheader.bin"));

        // Add to zip file
        var zippath = KiraPath.Combine(Application.persistentDataPath, newsid + ".kirapack");
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
                var V2chart = ProtobufHelper.Load<V2.Chart>(fs.GetFile(path));
                if (chartVersion.CanRead(V2chart.version))
                {
                    return V2chart.level;
                }
                throw new InvalidDataException("Failed to read with V2 format, fallback to old chart.");
            }
            catch
            {
                return ProtobufHelper.Load<Chart>(fs.GetFile(path)).level;
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

        var chartsets = db.GetAllChartSets();
        var mids = new HashSet<int>();

        foreach (var chart in chartsets)
        {
            try
            {
                var file = fs.GetFile(GetChartResource(chart.Sid, "cheader.bin"));
                var cheader = ProtobufHelper.Load<cHeader>(file);
                cheader.difficultyLevel = chart.Difficulties.ToList();
                songList.cHeaders.Add(cheader);
                mids.Add(cheader.mid);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.Log($"Delete chart {chart.Sid} due to corrupted data.");
                DeleteChart(chart.Sid);
            }
        }

        foreach (var mid in mids)
        {
            try
            {
                var file = fs.GetFile(GetMusicResource(mid, "mheader.bin"));
                var mheader = ProtobufHelper.Load<mHeader>(file);
                songList.mHeaders.Add(mheader);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.Log($"Delete music {mid} due to corrupted data.");
                DeleteMusic(mid);
            }
        }

        Debug.Log(JsonConvert.SerializeObject(songList, Formatting.Indented));
        // ProtobufHelper.Save(songList, SongListPath);

        // Populate dic data
        foreach (var music in songList.mHeaders)
            musicDic[music.mid] = music;

        foreach (var chart in songList.cHeaders)
            chartDic[chart.sid] = chart;

        onSongListRefreshed.Invoke();
    }

    /*
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

                    var dir = KiraPath.Combine(DataDir, entry.FullName.Replace(entry.Name, ""));
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    ProtobufHelper.Save(obj, KiraPath.Combine(DataDir, entry.FullName.Replace(".json", ".bin")));
                }
            }
        }
    }
    */

    public async UniTask<int> LoadKiraPack(FileInfo file)
    {
        //string path = KiraPath.Combine(FSDir, Guid.NewGuid().ToString("N") + ".kpak");

        if (!file.Exists)
            return -1;

        Debug.Log($"Load kirapack: {file.FullName}");

        try
        {
            //File.Move(file.FullName, path);
            //fs.AddSearchPath(path);

            // Load charts
            loadingBlocker.Show("DataLoader.LoadKirapack".L(KiraPath.GetFileName(file.Name)), showProgress: true);

            // Find sid
            string path = file.FullName;
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
            await UniTask.WaitForEndOfFrame();

            // Run actual task
            var task = new ExtractKirapackTask(path, fs, db, this, true);
            loadingBlocker.SetProgress(task);
            await task.Run();

            loadingBlocker.Close();
            return ret;
        }
        catch (Exception e)
        {
            messageBannerController.ShowMsg(LogLevel.ERROR, "Exception.DataLoader.CantLoadKirapack".L(file.Name, e.Message));
            Debug.LogException(e);
            return -1;
        }
    }

    public async UniTask<bool> LoadAllKiraPackFromInbox()
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
                if (file.Extension == ".kirapack")
                {
                    int tmp = await LoadKiraPack(file);
                    if (tmp != -1)
                    {
                        cl_lastsid.Set(tmp);
                        LoadSuccess = true;
                        messageBannerController.ShowMsg(LogLevel.OK, "DataLoader.LoadedKiraPack".L(file.Name));
                    }
                    file.Delete();
                }
            }
        }
        catch (Exception e)
        {
            messageBannerController.ShowMsg(LogLevel.ERROR, "Exception.Unknown".L(e.Message), false);
        }
        return LoadSuccess;
    }

    private async UniTask CopyFileFromStreamingAssetsToPersistentDataPath(string relativePath)
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
            string directory = KiraPath.GetDirectoryName(Application.persistentDataPath + relativePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            using (var writer = File.Create(Application.persistentDataPath + relativePath))
            {
                writer.Write(webRequest.downloadHandler.data, 0, webRequest.downloadHandler.data.Length);
            }
            Debug.Log($"Copy File {relativePath} {!webRequest.isNetworkError}");
        }
    }

    private void MoveFiles(string oldPrefix, string newPrefix, bool overwrite)
    {
        // Sanity check
        var oldFiles = fs.ListDirectory(oldPrefix);
        var existingFilenames = new HashSet<string>(oldFiles.Select(i => KiraPath.GetFileName(i.Name)));
        var bus = fs.ListDirectory(newPrefix).Where(file => existingFilenames.Contains(KiraPath.GetFileName(file.Name))).ToArray();
        if (bus.Length > 0 && !overwrite)
            throw new InvalidOperationException($"Conflict: {bus[0].Name} will be overwritten.");
        foreach (var file in bus)
            file.Delete();
        //var packs = new HashSet<string>();
        foreach (var file in oldFiles)
        {
            file.Name = KiraPath.Combine(newPrefix, Path.GetFileName(file.Name));
            //packs.Add(file.RootPath);
        }
        /*
        foreach (var pack in packs)
        {
            fs.FlushPak(pack);
        }*/
    }

    public void MoveChart(int oldSid, int newSid, bool overwrite = true)
    {
        if (oldSid == newSid)
            return;
        MoveFiles(ChartDir + oldSid, ChartDir + newSid, overwrite);
        // Handle rename
        foreach (var chart in chartList)
        {
            if (chart.sid == oldSid)
            {
                chartDic.Remove(oldSid);
                chart.sid = newSid;
                SaveHeader(chart);
                chartDic[newSid] = chart;
                if (cl_lastsid == oldSid)
                    cl_lastsid.Set(newSid);
                break;
            }
        }
    }

    public void MoveMusic(int oldMid, int newMid, bool overwrite = true)
    {
        if (oldMid == newMid)
            return;
        string newPrefix = MusicDir + newMid;
        MoveFiles(MusicDir + oldMid, newPrefix, overwrite);
        // Rename the song file
        var files = fs.ListDirectory(newPrefix + "/")
            .Where(file => KiraPath.GetFileName(file.Name).StartsWith(oldMid + ".")).ToArray();
        foreach (var file in files)
        {
            file.Name = file.Name.Replace(newPrefix + "/" + oldMid + ".", newPrefix + "/" + newMid + ".");
        }
        // Handle rename
        foreach (var song in musicList)
        {
            if (song.mid == oldMid)
            {
                musicDic.Remove(oldMid);
                song.mid = newMid;
                SaveHeader(song);
                musicDic[newMid] = song;
                break;
            }
        }
        foreach (var chart in chartList)
        {
            if (chart.mid == oldMid)
            {
                chart.mid = newMid;
                SaveHeader(chart);
            }
        }
    }

    private void DeleteFiles(string prefix)
    {
        //var packs = new HashSet<string>();
        var files = fs.ListDirectory(prefix).ToArray();
        foreach (var file in files)
        {
            file.Delete();
            //packs.Add(file.RootPath);
        }
        /*
        foreach (var pack in packs)
        {
            fs.FlushPak(pack);
        }*/
    }

    public void DeleteChart(int[] sids)
    {
        foreach (int sid in sids)
            DeleteFiles(ChartDir + sid);
        db.RemoveChartSets(sids);
    }

    public void DeleteChart(int sid)
    {
        DeleteChart(new int[] { sid });
    }

    public void DeleteDifficulty(int sid, Difficulty difficulty)
    {
        //var packs = new HashSet<string>();
        string luafile = difficulty.Lower() + ".lua";
        string binfile = difficulty.Lower() + ".bin";
        var files = fs.ListDirectory(KiraPath.Combine(ChartDir, sid.ToString())).Where(file =>
        {
            if (file.Name.ToLower().EndsWith(luafile) || file.Name.ToLower().EndsWith(binfile))
                return true;
            return false;
        }).ToArray();
        foreach (var file in files)
        {
            file.Delete();
            //packs.Add(file.RootPath);
        }
        db.RemoveChartSetDifficulty(sid, difficulty);
        /*
        foreach (var pack in packs)
        {
            fs.FlushPak(pack);
        }
        */
    }

    public void DeleteMusic(int mid)
    {
        DeleteFiles(MusicDir + mid);
    }
}
