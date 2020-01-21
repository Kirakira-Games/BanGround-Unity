﻿using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FMOD;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class AudioManager : MonoBehaviour
{
    FMOD.System System;

    ChannelGroup SEChannelGroup;
    ChannelGroup BGMChannelGroup;

    Channel CurrentBGMChannel;

    List<Sound> LoadedSound = new List<Sound>();

    public bool loading = true;//bgm will not start untill the gate open

    void Awake()
    {
        Factory.System_Create(out System);
        var result = System.init(1024, INITFLAGS.NORMAL, IntPtr.Zero);

        result = System.createChannelGroup("SoundEffects", out SEChannelGroup);
        SEChannelGroup.setVolume(LiveSetting.seVolume);

        result = System.createChannelGroup("BackgroundMuisc", out BGMChannelGroup);
    }

    void Update()
    {
        System.update();

        if (!loading && !GetPlayStatus())
        {
            loading = true;
            StartCoroutine(ShowResult());
        }
    }

    public Sound PrecacheSound(TextAsset asset)
    {
        var bytes = asset.bytes;

        Sound sound;

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(exinfo);
        exinfo.length = (uint)bytes.Length;

        var result = System.createSound(bytes, MODE._2D | MODE.OPENMEMORY, ref exinfo, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    public Sound PrecacheSound(byte[] bytes)
    {
        Sound sound;

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(exinfo);
        exinfo.length = (uint)bytes.Length;

        var result = System.createSound(bytes, MODE._2D | MODE.OPENMEMORY, ref exinfo, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    public Sound PrecacheSound(string path)
    {
        Sound sound;

        var result = System.createSound(path, MODE._2D, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    public void UnloadSound(Sound sound)
    {
        LoadedSound.Remove(sound);
        sound.release();
    }

    public Channel PlaySE(Sound sound)
    {
        Channel channel;

        System.playSound(sound, SEChannelGroup, false, out channel);

        return channel;
    }

    public Channel PlayBGM(Sound sound)
    {
        Channel channel;
        
        System.playSound(sound, BGMChannelGroup, false, out channel);
        
        CurrentBGMChannel = channel;
        return channel;
    }

    public int GetBGMPlaybackTime()
    {
        uint pos;
        CurrentBGMChannel.getPosition(out pos, TIMEUNIT.MS);

        return (int)pos + LiveSetting.audioOffset;
    }

    public bool GetPlayStatus()
    {
        bool isPlaying;
        CurrentBGMChannel.isPlaying(out isPlaying);
        //UnityEngine.Debug.Log(isPlaying);
        return isPlaying;
    }

    IEnumerator ShowResult()
    {
        Text gateTxt = GameObject.Find("GateText").GetComponent<Text>();
        switch (ResultsGetter.GetClearMark())
        {
            case ClearMarks.AP:
                gateTxt.text = "ALL PERFECT";//TODO:switch to image
                break;
            case ClearMarks.FC:
                gateTxt.text = "FULL COMBO";//TODO:switch to image
                break;
            case ClearMarks.CL:
                gateTxt.text = "CLEAR";//TODO:switch to image
                break;
            case ClearMarks.F:
                gateTxt.text = "FAILED";//TODO:switch to image
                break;
        }
        GameObject.Find("GateCanvas").GetComponent<Animator>().SetBool("SongOver", true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("Result");
    }

    void OnApplicationQuit()
    {
        foreach (var sound in LoadedSound)
            sound.release();

        SEChannelGroup.release();
        BGMChannelGroup.release();
        System.release();
    }
}