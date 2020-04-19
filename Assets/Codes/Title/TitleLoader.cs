﻿using System;
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
    public TextAsset[] voice;
    public Text Title;
    public Text touchStart;

    public static TitleLoader instance;

    public ISoundTrack music;
    private ISoundEffect banGround;

    private void Awake()
    {
        instance = this;
        StartCoroutine(CheckUpdate());
        StartCoroutine(DataLoader.Init());
    }

    private void Start()
    {

        StartCoroutine(PlayTitle());

        //MessageBoxController.ShowMsg(LogLevel.INFO, SystemInfo.deviceUniqueIdentifier.Substring(0, 8));
    }

    IEnumerator PlayTitle()
    {
        //yield return new WaitForSeconds(0.5f);
        music = AudioManager.Instance.PlayLoopMusic(titleMusic.bytes);
        music.SetVolume(0.7f);
        yield return new WaitForSeconds(3f);

        banGround = AudioManager.Instance.PrecacheSE(voice[UnityEngine.Random.Range(0,voice.Length)].bytes);
        banGround.PlayOneShot();


    }

    IEnumerator CheckUpdate()
    {
        //MessageBoxController.ShowMsg(LogLevel.INFO, VersionCheck.CheckUpdate);
        TouchEvent te = GameObject.Find("TouchStart").GetComponent<TouchEvent>();
        var check = VersionCheck.Instance;
        yield return StartCoroutine(check.GetVersionInfo());

        if (check == null || check.version == null || check.version.status == false) 
        {
            //网络错误
            MessageBoxController.ShowMsg(LogLevel.ERROR, VersionCheck.CheckError, false);
            te.waitingUpdate = false; // 椰叶先别强制更新罢
        }
        else if (check.version.data.has)
        {
            //有更新
            if (check.version.data.force)
            {
                string result = string.Format(VersionCheck.UpdateForce, check.version.data.version);
                //强制更新
                MessageBoxController.ShowMsg(LogLevel.ERROR, result, false);
            }
            else
            {
                string result = string.Format(VersionCheck.UpdateNotForce, check.version.data.version);
                //不强制更新
                MessageBoxController.ShowMsg(LogLevel.OK, result, true);
                te.waitingUpdate = false;
            }
        }
        else
        {
            //无更新
            //MessageBoxController.ShowMsg(LogLevel.OK, VersionCheck.NoUpdate, true);
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