﻿using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Un4seen.Bass;
using System.Runtime.InteropServices;

class AudioManager : MonoBehaviour
{
    internal List<int> LoadedSound = new List<int>();
    private int bgmId = 0;
    private int bgmCid = 0;

    public bool loading = true;//bgm will not start untill the gate open
    public bool isInGame;

    private int lastPos = -1;
    private float lastUpdateTime = -1;

    void Awake()
    {
        Bass.BASS_Free();

        BASSInit flag = BASSInit.BASS_DEVICE_DEFAULT;
#if UNITY_ANDROID && !UNITY_EDITOR
        flag |= BASSInit.BASS_DEVICE_AUDIOTRACK;
#endif

        if (!Bass.BASS_Init(-1, AudioSettings.outputSampleRate, flag, IntPtr.Zero))
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }
    }


    void Update()
    {
        //print(LoadedSound.Count);
        if (isInGame)
        {
            if (!loading && !GetPlayStatus())
            {
                loading = true;
                GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish();
                
            }
        }
    }

    public IEnumerator DelayPlayBGM(byte[] audioData, float seconds)
    {
        var BGM = PrecacheSound(audioData);
        loading = true;
        lastPos = (int)((Time.time + seconds) * 1000);
        yield return new WaitForSeconds(seconds);
        PlayBGM(BGM);
        loading = false;
        lastPos = -999;
    }

    public int PrecacheBGM(TextAsset internalFile, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var id = Bass.BASS_SampleLoad(internalFile.bytes, 0, internalFile.bytes.Length, 1, flags);

        if (id == 0)
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        return id;
    }

    public int PrecacheSound(TextAsset internalFile, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var id = Bass.BASS_SampleLoad(internalFile.bytes, 0, internalFile.bytes.Length, 65535, flags);

        if (id == 0)
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        return id;
    }

    public int PrecacheSound(byte[] bytes, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var id = Bass.BASS_SampleLoad(bytes, 0, bytes.Length, 65535, flags);

        if (id == 0)
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        return id;
    }

    public void UnloadSound(int sound)
    {
        LoadedSound.Remove(sound);
        Bass.BASS_SampleFree(sound);
    }

    public int PlaySE(int sound)
    {
        var cid = Bass.BASS_SampleGetChannel(sound, false);
        Bass.BASS_ChannelSetAttribute(cid, BASSAttribute.BASS_ATTRIB_VOL, LiveSetting.seVolume);
        Bass.BASS_ChannelPlay(cid, false);

        return cid;
    }

    public int PlayBGM(int sound)
    {
        if (bgmCid != 0)
            Bass.BASS_ChannelStop(bgmCid);

        var cid = Bass.BASS_SampleGetChannel(sound, false);
        Bass.BASS_ChannelSetAttribute(cid, BASSAttribute.BASS_ATTRIB_VOL, LiveSetting.bgmVolume);
        Bass.BASS_ChannelPlay(cid, true);

        bgmId = sound;
        bgmCid = cid;

        return cid;
    }

    public int PlayPreview(int sound)
    {
        return PlayBGM(sound);
    }

    public int GetBGMPlaybackTime()
    {
        if (loading) 
            return (int)(Time.time * 1000) - lastPos + LiveSetting.audioOffset;

        var pos = Bass.BASS_ChannelGetPosition(bgmCid);
        var time = (int)(Bass.BASS_ChannelBytes2Seconds(bgmCid, pos) * 1000);

        if (GetPlayStatus())
        {
            if (time != lastPos)
            {
                lastPos = time;
                lastUpdateTime = Time.time;
            }
            else
            {
                return (int)((Time.time - lastUpdateTime) * 1000) + lastPos + LiveSetting.audioOffset;
            }
        }
        else
        {
            return lastPos + LiveSetting.audioOffset;
        }

        return lastPos + LiveSetting.audioOffset;
    }

    /// <summary>
    /// 获取bgm的播放状态（暂停状态下也应返回true）
    /// </summary>
    /// <returns></returns>
    public bool GetPlayStatus()
    {
        var status = Bass.BASS_ChannelIsActive(bgmCid);
        //return status == BASSActive.BASS_ACTIVE_PLAYING || GetPauseStatus();
        return status != BASSActive.BASS_ACTIVE_STOPPED;
    }

    public bool GetPauseStatus()
    {
        var status = Bass.BASS_ChannelIsActive(bgmCid);
        return status == BASSActive.BASS_ACTIVE_PAUSED;
    }

    public void PauseBGM()
    {
        print("pause");
        Bass.BASS_ChannelPause(bgmCid);
    }

    public void ResumeBGM()
    {
        Bass.BASS_ChannelPlay(bgmCid, false);
    }

    public void StopBGM()
    {
        Bass.BASS_ChannelStop(bgmCid);
    }

    void OnApplicationQuit()
    {
        foreach (var sound in LoadedSound)
            Bass.BASS_SampleFree(sound);

        Bass.BASS_Free();
    }

    private void OnDestroy()
    {
        foreach (var sound in LoadedSound)
            Bass.BASS_SampleFree(sound);

        Bass.BASS_Free();
    }
}