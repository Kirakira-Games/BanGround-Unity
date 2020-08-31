﻿using AudioProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

class LatencyOffsetGuide : MonoBehaviour
{
    public Text[] txtOffsets;
    public Text avgOffset;
    public Text okText;

    public Button resetButton;
    public Button okButton;
    public Button backButton;
    public Button touchButton;
    public Button openButton;

    int[] offsets = { 0, 0, 0, 0 };

#if USE_SE
    public TextAsset kickSound;
    public TextAsset hatSound;
    public TextAsset snareSound;

    ISoundEffect kick;
    ISoundEffect hat;
    ISoundEffect snare;
#else
    public TextAsset bgmSound;
    ISoundEffect bgm;
#endif

    [Inject]
    IAudioManager audioManager;

    Stopwatch watch = new Stopwatch();

    int[] timePoints = { 2000, 4000, 6000, 8000 };

    private void Awake()
    {
        touchButton.onClick.AddListener(OnClick);
        openButton.onClick.AddListener(OnOpen);
        resetButton.onClick.AddListener(OnReset);
        backButton.onClick.AddListener(OnClose);

        gameObject.SetActive(false);
    }

    public async void OnOpen()
    {
        gameObject.SetActive(true);

#if USE_SE
        kick = await audioManager.PrecacheSE(kickSound.bytes);
        hat = await audioManager.PrecacheSE(hatSound.bytes);
        snare = await audioManager.PrecacheSE(snareSound.bytes);
#else
        bgm = await audioManager.PrecacheSE(bgmSound.bytes);
#endif

        SelectManager_old.instance.previewSound?.Pause();

        OnReset();
    }

    public void OnClick()
    {
        var time = watch.ElapsedMilliseconds;

        var subtractedOffsets = new List<(int, long)>();

        for(int i = 0; i < timePoints.Length; i++)
            subtractedOffsets.Add((i, timePoints[i] - time));

        subtractedOffsets.Sort((a, b) => Math.Abs(a.Item2).CompareTo(Math.Abs(b.Item2)));

        var (index, value) = subtractedOffsets[0];

        offsets[index] = (int)value;
        txtOffsets[index].text = value.ToString();
    }

    public void OnOK()
    {
        OnClose();
    }

    public void OnReset()
    {
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(OnStart);
        okText.text = "offset.start.btn.text";
        avgOffset.text = "";

        resetButton.interactable = false;
        touchButton.interactable = false;

        txtOffsets.All(text =>
        {
            text.text = "";
            return true;
        });

        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = 0;
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
        SelectManager_old.instance.previewSound?.Play();

#if USE_SE
        kick.Dispose();
        hat.Dispose();
        snare.Dispose();
#else
        bgm.Dispose();
#endif
    }

    public async void OnStart()
    {
        watch.Reset();
        watch.Start();

        backButton.interactable = false;
        okButton.interactable = false;
        resetButton.interactable = false;
        touchButton.interactable = true;

        await PlayDrumSound();

        touchButton.interactable = false;
        backButton.interactable = true;
        okButton.interactable = true;
        resetButton.interactable = true;
        watch.Stop();

        var avg = 0;

        foreach (var offset in offsets)
            avg += offset;

        avg /= offsets.Length;

        avgOffset.text = avg.ToString();

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(OnOK);
        okText.text = "offset.ok.btn.text";
    }

    public async Task PlayDrumSound()
    {
#if !USE_SE
        await Task.Delay(500);
        bgm.PlayOneShot();
        await Task.Delay(8000);
#else
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        snare.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        snare.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        snare.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        kick.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
        snare.PlayOneShot();
        await Task.Delay(500);
        hat.PlayOneShot();
        await Task.Delay(500);
#endif
    }
}