using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;
using Un4seen.Bass;
using AudioProvider;

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
    }

    private void OnDestroy()
    {
        music.Dispose();
        banGround.Dispose();
        LocalizedStrings.Instanse.ReloadLanguageFile(LiveSetting.language);
        LocalizedText.ReloadAll();
    }
}