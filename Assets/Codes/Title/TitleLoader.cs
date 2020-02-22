using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;

public class TitleLoader : MonoBehaviour
{
    public TextAsset titleMusic;
    public TextAsset voice;

    private void Awake()
    {
        StartCoroutine(DataLoader.Init());
    }

    private void Start()
    {
        StartCoroutine(PlayTitle());

        MessageBoxController.Instance.ShowMsg(LogLevel.INFO, "Load Device Audio Info Success");
        MessageBoxController.Instance.ShowMsg(LogLevel.INFO, "SampleRate: " + AppPreLoader.sampleRate.ToString() + "  BufferSize: " + AppPreLoader.bufferSize.ToString());
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
}