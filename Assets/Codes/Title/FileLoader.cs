using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;

public class FileLoader : MonoBehaviour
{
    public TextAsset titleMusic;
    public TextAsset voice;

    private void Start()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(InitCharts());
#else
        LoadSongListFromFile(Application.streamingAssetsPath + "/SongList.bin");
#endif
        LoadLiveSettingFile();

        GameObject.Find("AudioMgr").AddComponent<AudioManager>();
        StartCoroutine(PlayTitle());
    }

    IEnumerator PlayTitle()
    {
        yield return new WaitForSeconds(0.5f);
        var music = gameObject.AddComponent<BassAudioSource>();
        music.clip = titleMusic;
        music.loop = true;
        music.playOnAwake = true;

        yield return new WaitForSeconds(3f);

        var banGround = gameObject.AddComponent<BassAudioSource>();
        banGround.clip = voice;
        banGround.playOnAwake = true;
    }

    void LoadSongListFromFile(string path)
    {
        //string songListJson;
        if (File.Exists(path))
        {
            //songListJson = File.ReadAllText(path);
            //LiveSetting.songList = JsonConvert.DeserializeObject<SongList>(songListJson);
            LiveSetting.songList = ProtobufHelper.Load<SongList>(path);
            print("SongList Loaded");
        }
        else
        {
            Debug.LogError("SongList.bin not found! pls gennerate it in editor");
        }
    }

    void LoadLiveSettingFile()
    {
        if (File.Exists(LiveSetting.settingsPath))
        {
            string sets = File.ReadAllText(LiveSetting.settingsPath);
            LiveSettingTemplate loaded = JsonConvert.DeserializeObject<LiveSettingTemplate>(sets);
            loaded.ApplyToLiveSetting();
        }
        else
        {
            Debug.LogWarning("Live setting file not found");
        }
    }

    private IEnumerator InitCharts()
    {
        //Load List From StreamingAssetsPath And Copy a Temp Version to PersistentDataPath
        UnityWebRequest webRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/SongList.bin");
        yield return webRequest.SendWebRequest();
        byte[] songData = webRequest.downloadHandler.data;
        File.WriteAllBytes(Application.persistentDataPath + "/SongList.bin.tmp", songData);

        //Load List
        SongList newList = ProtobufHelper.Load<SongList>(Application.persistentDataPath + "/SongList.bin.tmp");
        SongList oldList;
        if (File.Exists(Application.persistentDataPath + "/SongList.bin"))
            oldList = ProtobufHelper.Load<SongList>(Application.persistentDataPath + "/SongList.bin");
        else oldList = null;
        LiveSetting.songList = oldList;

        if (oldList != null && newList.GenerateDate == oldList.GenerateDate) yield break;
        //need update
        else
        {
            //Update : Write New SongList
            File.WriteAllBytes(Application.persistentDataPath + "/SongList.bin", songData);
            LiveSetting.songList = newList;

            //Prepare FileList
            List<string> files = new List<string>();
            foreach (Header h in LiveSetting.songList.songs)
            {
                files.Add("/TestCharts/" + h.DirName + "/header.bin");
                files.Add("/TestCharts/" + h.DirName + "/bgm.ogg");
                foreach (Chart c in h.charts)
                {
                    files.Add("/TestCharts/" + h.DirName + "/" + c.fileName + ".bin");
                    if (!string.IsNullOrWhiteSpace(c.backgroundFile)) files.Add("/TestCharts/" + h.DirName + "/" + c.fileName + ".jpg");
                }
            }

            //Copy File
            for (int i = 0; i < files.Count; i++)
            {
                if (!File.Exists(Application.persistentDataPath + files[i]))
                    StartCoroutine(CopyFileFromStreamingAssetsToPersistentDataPath(files[i]));
            }
        }

        //string newJson = webRequest.downloadHandler.text;
        //list = JsonConvert.DeserializeObject<SongList>(newJson);
        //string fileVersion = list.GenerateDate;//read from streaming assets
        //string songListJsonOLD;

        //if (File.Exists(Application.persistentDataPath + "/SongList.bin"))
        //{
        //    songListJsonOLD = File.ReadAllText(Application.persistentDataPath + "/SongList.bin");
        //    list = JsonConvert.DeserializeObject<SongList>(songListJsonOLD);
        //    if (list.GenerateDate == fileVersion)
        //    {
        //        LiveSetting.songList = list;
        //        yield break;
        //    }
        //}

        //yield return StartCoroutine(CopyFileFromStreamingAssetsToPersistentDataPath("/SongList.bin"));

        //songListJsonOLD = File.ReadAllText(Application.persistentDataPath + "/SongList.bin");
        //list = JsonConvert.DeserializeObject<SongList>(songListJsonOLD);
        //if (list.GenerateDate == fileVersion)
        //{
        //    LiveSetting.songList = list;
        //    print("songList updated");
        //}
        //else
        //{
        //    Debug.LogError("NMSL");
        //}


        /*string[] files =
        {

            "/TestCharts/85/bgm.mp3",
            "/TestCharts/85/header.json",
            "/TestCharts/85/0.json",
            "/TestCharts/85/1.json",
            "/TestCharts/85/2.json",

            //"/TestCharts/112/",

            //"/TestCharts/128/",

            //"/TestCharts/175/",

            "/TestCharts/243/bgm.mp3",
            "/TestCharts/243/header.json",
            "/TestCharts/243/0.json",
            "/TestCharts/243/1.json",
        };
        */

    }

    private IEnumerator CopyFileFromStreamingAssetsToPersistentDataPath(string relativePath)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Application.streamingAssetsPath + relativePath))
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