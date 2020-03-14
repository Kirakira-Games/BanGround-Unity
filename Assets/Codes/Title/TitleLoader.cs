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
        StartCoroutine(CheckUpdate());
        StartCoroutine(DataLoader.Init());
    }

    private void Start()
    {
        if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
        {
            voice = Resources.Load<TextAsset>("Sound/voice/LetTheBassKick");
        }

        StartCoroutine(PlayTitle());
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
            if (!Directory.Exists($"{Application.persistentDataPath}/Inbox"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Inbox");
            if (!Directory.Exists($"{Application.persistentDataPath}/data/chart/233333"))
            {
                File.WriteAllBytes($"{Application.persistentDataPath}/Inbox/BBKKBKK_Min_Commit_c8ecd6fa71.kirapack", Resources.Load<TextAsset>("BBKKBKK_Min_Commit_c8ecd6fa71.kirapack").bytes);
#if UNITY_ANDROID
                DataLoader.LoadAllKiraPackFromInbox();
#endif
            }
        }
    }

    IEnumerator CheckUpdate()
    {
        MessageBoxController.ShowMsg(LogLevel.INFO, "Checking for Update · · · · · ·");
        TouchEvent te = GameObject.Find("TouchStart").GetComponent<TouchEvent>();
        var check = new VersionCheck("http://yapi.banground.fun/mock/28/");
        yield return StartCoroutine(check.GetVersionInfo());

        Debug.Log(check.version.data.version);
        if (check.version == null) 
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, "Check Version ERROR!", false);
            te.waitingUpdate = false;
        } 
        else if (check.version.data.version != VersionCheck.CurrentVersion)
        {
            if (check.version.data.forceUpdate)
            {
                MessageBoxController.ShowMsg(LogLevel.ERROR, $"You Must Update to Latest Version:{check.version.data.version}", false);
                te.gameObject.SetActive(false);
            }
            else
            {
                MessageBoxController.ShowMsg(LogLevel.OK, $"Latest Version Found:{check.version.data.version}");
                te.waitingUpdate = false;
            }
        }
        else
        {
            MessageBoxController.ShowMsg(LogLevel.OK, $"You Have the Latest Version");
            te.waitingUpdate = false;
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