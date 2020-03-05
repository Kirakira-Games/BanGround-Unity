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

public class TitleLoader : MonoBehaviour
{
    public TextAsset titleMusic;
    public TextAsset voice;

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
        var music = gameObject.AddComponent<BassAudioSource>();
        music.clip = titleMusic;
        music.loop = true;
        music.playOnAwake = true;

        yield return new WaitForSeconds(3f);

        var banGround = gameObject.AddComponent<BassAudioSource>();
        banGround.clip = voice;
        banGround.playOnAwake = true;
    }

    private void OnDestroy()
    {
        LocalizedStrings.Instanse.ReloadLanguageFile(LiveSetting.language);
        LocalizedText.ReloadAll();

    }
}