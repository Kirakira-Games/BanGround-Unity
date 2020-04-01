using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;
using AudioProvider;
using UnityEngine.UI;

public class TitleLoader : MonoBehaviour
{
    public TextAsset titleMusic;
    public TextAsset voice;

    private ISoundTrack music;
    private ISoundEffect banGround;

    private void Awake()
    {
        DataLoader.Init();
    }

    private void Start()
    {
        if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
        {
            voice = Resources.Load<TextAsset>("Sound/voice/LetTheBassKick");
        }

        StartCoroutine(PlayTitle());

        //if (Application.platform != RuntimePlatform.Android) return;
    }

    IEnumerator PlayTitle()
    {
        yield return new WaitForSeconds(0.5f);
        music = AudioManager.Instance.PlayLoopMusic(titleMusic.bytes);

        yield return new WaitForSeconds(3f);

        banGround = AudioManager.Instance.PrecacheSE(voice.bytes);
        banGround.PlayOneShot();

        if(DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
        {
            GameObject.Find("Title").GetComponent<Text>().text = "Let the bass kick!";
            var e = GameObject.Find("TouchStart").transform.GetEnumerator();
            e.MoveNext();
            (e.Current as Transform).gameObject.GetComponent<Text>().text = "Bass Bass Kick Kick Bass Kick Kick";
            if (!Directory.Exists($"{Application.persistentDataPath}/InBox"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/InBox");
            if (!Directory.Exists($"{Application.persistentDataPath}/data/chart/233333"))
                File.WriteAllBytes($"{Application.persistentDataPath}/InBox/BBKKBKK_Min_Commit_c8ecd6fa71.kirapack", Resources.Load<TextAsset>("BBKKBKK_Min_Commit_c8ecd6fa71.kirapack").bytes);
        }
    }

    private void OnDestroy()
    {
        music?.Dispose();
        banGround?.Dispose();
        LocalizedStrings.Instanse.ReloadLanguageFile(LiveSetting.language);
        LocalizedText.ReloadAll();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) music?.Play();
        else music?.Pause();
    }
}