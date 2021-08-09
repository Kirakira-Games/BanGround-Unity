using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using V2;

public interface IDataLoader
{
    UnityEvent onSongListRefreshed { get; }
    List<cHeader> chartList { get; }
    bool loaded { get; }
    List<mHeader> musicList { get; }
    int LastImportedSid { get; set; }

    string BuildKiraPack(cHeader header);
    UniTask CopyFileFromStreamingAssetsToPersistentDataPath(string relativePath);
    //void ConvertJsonToBin(DirectoryInfo dir);
    void DuplicateKiraPack(cHeader header);
    int GenerateMid();
    int GenerateSid();
    (string, int) GetBackgroundPath(int sid, bool forceImg = true);
    cHeader GetChartHeader(int sid);
    int GetChartLevel(string path);
    string GetChartResource(int sid, string file);
    string GetChartPath(int sid, Difficulty difficulty);
    string GetChartScriptPath(int sid, Difficulty difficulty);
    int GetMidBySid(int sid);
    mHeader GetMusicHeader(int mid);
    string GetMusicPath(int mid);
    string GetMusicResource(int mid, string filename);
    void Init();
    void InitFileSystem();
    UniTask<bool> LoadAllKiraPackFromInbox();
    T LoadChart<T>(int sid, Difficulty difficulty) where T : IExtensible;
    UniTask<int> LoadKiraPack(FileInfo file);
    bool MusicExists(int mid);
    void RefreshSongList();
    void SaveChart(V2.Chart chart, int sid, Difficulty difficulty);
    void SaveChartScript(string script, int sid, Difficulty difficulty);
    void SaveHeader(cHeader header, string coverExt = null, byte[] cover = null);
    void SaveHeader(mHeader header);
    void SaveHeader(mHeader header, byte[] oggFile);
    void MoveChart(int oldSid, int newSid, bool overwrite = true);
    void MoveMusic(int oldMid, int newMid, bool overwrite = true);
    void DeleteChart(int[] sids);
    void DeleteChart(int sid);
    void DeleteDifficulty(int sid, Difficulty difficulty);
    void DeleteMusic(int mid);
}
