using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx.Async;
using UnityEngine.Events;

public interface IDataLoader
{
    UnityEvent onSongListRefreshed { get; }
    List<cHeader> chartList { get; }
    bool loaded { get; }
    List<mHeader> musicList { get; }
    int LastImportedSid { get; set; }

    string BuildKiraPack(cHeader header);
    void ConvertJsonToBin(DirectoryInfo dir);
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
    UniTaskVoid Init();
    void InitFileSystem();
    bool LoadAllKiraPackFromInbox();
    T LoadChart<T>(int sid, Difficulty difficulty) where T : IExtensible;
    int LoadKiraPack(FileInfo file);
    bool MusicExists(int mid);
    void RefreshSongList();
    void SaveChart<T>(T chart, int sid, Difficulty difficulty) where T : IExtensible;
    void SaveChartScript(string script, int sid, Difficulty difficulty);
    void SaveHeader(cHeader header);
    void SaveHeader(mHeader header);
    void SaveHeader(mHeader header, byte[] oggFile);
}