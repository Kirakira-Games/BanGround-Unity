using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;

public class FileLoader : MonoBehaviour
{
    private void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        InitCharts();
        #endif
    }

    private void InitCharts()
    {
        string fileVersion = "/FileVersion20200131001.txt";
        if (File.Exists(Application.persistentDataPath + fileVersion)) return;
        else
        {
            StartCoroutine(CopyFileFromStreamingAssetsToPersistentDataPath(fileVersion));
        }

        string[] files =
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

        for (int i = 0; i < files.Length; i++)
        {
            if (!File.Exists(Application.persistentDataPath + files[i])) 
                StartCoroutine(CopyFileFromStreamingAssetsToPersistentDataPath(files[i]));
        }
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