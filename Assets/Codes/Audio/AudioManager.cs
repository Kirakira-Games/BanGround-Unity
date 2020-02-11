﻿using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using System.Runtime.InteropServices;

public class LoopingBassMemStream : BassMemStream
{
    public int LoopStart;
    public int LoopEnd;
    public bool Fade;

    private float OrigVolume;

    public LoopingBassMemStream(GCHandle handle, int id, float start = -1, float end = -1, bool fade = false) : base(handle, id)
    {
        LoopStart = (int)start * 1000;
        LoopEnd = (int)end * 1000;

        if (start == -1 || end == -1)
        {
            LoopStart = 0;
            var bytesLen = Bass.BASS_ChannelGetLength(ID);
            LoopEnd = (int)(Bass.BASS_ChannelBytes2Seconds(ID, bytesLen) * 1000);
        }

        Position = LoopStart;
        Fade = fade;
    }

    public new void Play(bool restart = false)
    {
        OrigVolume = Volume;

        base.Play(restart);
    }

    public void OnUpdate()
    {
        if (Status != BASSActive.BASS_ACTIVE_PLAYING)
            return;

        int diff = LoopEnd - Position;

        if (diff < 2000)
        {
            float volume = Math.Max(diff, 0) / 2000.0f;
            Volume = OrigVolume * volume;

            if (diff <= -750)
            {
                Position = LoopStart;
                Volume = OrigVolume;
            }
        }
    }
}

public class BassMemStream : IDisposable
{
    int id;
    GCHandle pinnedObject;

    public int ID { get { return id; } }
    public float Volume
    {
        get
        {
            float volume = 0.0f;
            Bass.BASS_ChannelGetAttribute(ID, BASSAttribute.BASS_ATTRIB_VOL, ref volume);

            return volume;
        }
        set
        {
            Bass.BASS_ChannelSetAttribute(ID, BASSAttribute.BASS_ATTRIB_VOL, value);
        }
    }

    public int Position
    {
        get
        {
            var pos = Bass.BASS_ChannelGetPosition(ID);
            var time = (int)(Bass.BASS_ChannelBytes2Seconds(ID, pos) * 1000);

            return time;
        }
        set
        {
            var pos = Bass.BASS_ChannelSeconds2Bytes(ID, value / 1000.0);
            Bass.BASS_ChannelSetPosition(ID, pos);
        }
    }

    public BASSActive Status
    {
        get
        {
            return Bass.BASS_ChannelIsActive(ID);
        }
    }

    public BassMemStream(GCHandle handle, int id)
    {
        pinnedObject = handle;
        this.id = id;
    }

    public void Play(bool restart = false)
    {
        Bass.BASS_ChannelPlay(ID, restart);
    }

    public void Pause()
    {
        Bass.BASS_ChannelPause(ID);
    }

    public void Stop()
    {
        Bass.BASS_ChannelStop(ID);
    }

    public bool IsDisposed { get; private set; } = false;

    public void Dispose()
    {
        if (IsDisposed)
            throw new InvalidOperationException("Object already disposed!");

        IsDisposed = true;

        if (Bass.BASS_ChannelIsActive(ID) != BASSActive.BASS_ACTIVE_STOPPED)
            Stop();

        Bass.BASS_StreamFree(ID);
        pinnedObject.Free();
    }
}

class AudioManager : MonoBehaviour
{
    internal List<int> LoadedSound = new List<int>();
    private BassMemStream BGMStream;

    private List<LoopingBassMemStream> LoopingStreams = new List<LoopingBassMemStream>();

    public bool loading = true;//bgm will not start untill the gate open
    public bool isInGame;

    private int lastPos = -1;
    private float lastUpdateTime = -1;

    public static AudioManager Instanse { get; private set; }

    void Awake()
    {
        Bass.BASS_Free();

        BASSInit flag = BASSInit.BASS_DEVICE_DEFAULT;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (LiveSetting.enableAudioTrack)
        {
            flag |= BASSInit.BASS_DEVICE_AUDIOTRACK;
        }
#endif

        if (!Bass.BASS_Init(-1, AudioSettings.outputSampleRate, flag, IntPtr.Zero))
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        Instanse = this;
        DontDestroyOnLoad(Instanse.gameObject);
    }


    void Update()
    {
        for (int i = LoopingStreams.Count - 1; i >= 0; i--)
        {
            if (LoopingStreams[i].IsDisposed)
                LoopingStreams.RemoveAt(i);

            else 
                LoopingStreams[i].OnUpdate();
        }

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

    public int PrecacheBGM(TextAsset internalFile, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var id = Bass.BASS_SampleLoad(internalFile.bytes, 0, internalFile.bytes.Length, 1, flags);

        if (id == 0)
        {
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        return id;
    }

    public LoopingBassMemStream StreamLoopSound(byte[] file, float start = -1, float end = -1, bool fade = true, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var pinnedObject = GCHandle.Alloc(file, GCHandleType.Pinned);
        var pinnedObjectPtr = pinnedObject.AddrOfPinnedObject();

        var id = Bass.BASS_StreamCreateFile(pinnedObjectPtr, 0, file.Length, flags);

        var result = new LoopingBassMemStream(pinnedObject, id, start, end, fade);

        LoopingStreams.Add(result);
        return result;
    }

    public BassMemStream StreamSound(byte[] file, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        var pinnedObject = GCHandle.Alloc(file, GCHandleType.Pinned);
        var pinnedObjectPtr = pinnedObject.AddrOfPinnedObject();

        var id = Bass.BASS_StreamCreateFile(pinnedObjectPtr, 0, file.Length, flags);

        if(flags == BASSFlag.BASS_STREAM_DECODE)
        {
            id = BassFx.BASS_FX_TempoCreate(id, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_USE_QUICKALGO, 1);
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_OVERLAP_MS, 4);
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_SEQUENCE_MS, 30);
        }

        return new BassMemStream(pinnedObject, id);
    }

    public BassMemStream StreamSound(TextAsset internalFile, BASSFlag flags = BASSFlag.BASS_DEFAULT)
    {
        return StreamSound(internalFile.bytes, flags);
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

    public IEnumerator DelayPlayBGM(byte[] audioData, float seconds)
    {
        BGMStream = StreamSound(audioData, BASSFlag.BASS_STREAM_DECODE);
        loading = true;
        lastPos = (int)((Time.time + seconds) * 1000);
        yield return new WaitForSeconds(seconds);
        foreach(var mod in LiveSetting.attachedMods)
        {
            if (mod is AudioMod)
                (mod as AudioMod).ApplyMod(BGMStream);
        }

        BGMStream.Play();
        loading = false;
        lastPos = -999;
    }

    public int GetBGMPlaybackTime()
    {
        if (loading) 
            return (int)(Time.time * 1000) - lastPos + LiveSetting.audioOffset;

        var time = BGMStream.Position;

        if (GetPlayStatus())
        {
#if TIME_INTERPOLATION
            if (time != lastPos)
            {
                lastPos = time;
                lastUpdateTime = Time.time;
            }
            else
            {
                return (int)((Time.time - lastUpdateTime) * 1000) + lastPos + LiveSetting.audioOffset;
#else
            return time + LiveSetting.audioOffset;
#endif
        }
        else
        {
            return lastPos + LiveSetting.audioOffset;
        }
    }

    /// <summary>
    /// 获取bgm的播放状态（暂停状态下也应返回true）
    /// </summary>
    /// <returns></returns>
    public bool GetPlayStatus()
    {
        var status = BGMStream.Status;
        //return status == BASSActive.BASS_ACTIVE_PLAYING || GetPauseStatus();
        return status != BASSActive.BASS_ACTIVE_STOPPED;
    }

    public bool GetPauseStatus()
    {
        var status = BGMStream.Status;
        return status == BASSActive.BASS_ACTIVE_PAUSED;
    }

    public void PauseBGM()
    {
        print("pause");
        BGMStream.Pause();
    }

    public void ResumeBGM()
    {
        BGMStream.Play();
    }

    public void StopBGM()
    {
        BGMStream.Stop();
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