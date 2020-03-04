#pragma warning disable CS0414
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using System.Runtime.InteropServices;

[Obsolete("Fuck Audio")]
public class LoopingBassMemStream : BassMemStream
{
    public int LoopStart;
    public int LoopEnd;
    public bool Fade;

    private float OrigVolume;
    
    private static List<LoopingBassMemStream> AllInstanses = new List<LoopingBassMemStream>();

    public LoopingBassMemStream(GCHandle handle, int id, float start = -1, float end = -1, bool fade = false) : base(handle, id)
    {
    	AllInstanses.Add(this);
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
    
    public static void DisposeAll()
    {
    	foreach(var instanse in AllInstanses)
        {
        	if(instanse != null && !instanse.IsDisposed)
            {
            	instanse.Dispose();
            }
        }
        
        AllInstanses = new List<LoopingBassMemStream>();
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

[Obsolete("Fuck Audio")]
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
    
    public double Length
    {
    	get
        {
        	var bytes = Bass.BASS_ChannelGetLength(ID);
            var time = Bass.BASS_ChannelBytes2Seconds(ID, bytes);

            return time;
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
            return;
            //throw new InvalidOperationException("Object already disposed!");

        IsDisposed = true;

        if (Bass.BASS_ChannelIsActive(ID) != BASSActive.BASS_ACTIVE_STOPPED)
            Stop();

        Bass.BASS_StreamFree(ID);
        pinnedObject.Free();
    }
}

[Obsolete("Fuck Audio")]
class AudioManager : MonoBehaviour
{
    internal List<int> LoadedSound = new List<int>();
    internal BassMemStream BGMStream;

    private List<LoopingBassMemStream> LoopingStreams = new List<LoopingBassMemStream>();

    public bool loading = true;//bgm will not start untill the gate open
    public bool isInGame;
    public bool restart = false;

    internal int lastPos = -1;
    private float lastUpdateTime = -1;

    public static AudioManager Instanse { get; private set; }

    void Awake()
    {
        Bass.BASS_Free();

        BASSInit flag = BASSInit.BASS_DEVICE_DEFAULT;

//#if UNITY_ANDROID && !UNITY_EDITOR
//        if (LiveSetting.enableAudioTrack)
//        {
//            flag |= BASSInit.BASS_DEVICE_AUDIOTRACK;
//        }
        
//		if (AppPreLoader.init)
//        {
//            MessageBoxController.ShowMsg(LogLevel.INFO, "DEV_BUFFER: " + Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_DEV_BUFFER).ToString());
//            MessageBoxController.ShowMsg(LogLevel.INFO, "Set\"BUFFER\"To: " + (AppPreLoader.bufferSize * HandleValue_buffer.bufferSize[LiveSetting.bufferSize]).ToString());
//            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, (int)(AppPreLoader.bufferSize * HandleValue_buffer.bufferSize[LiveSetting.bufferSize]));
//        }

//        if (!Bass.BASS_Init(-1, AppPreLoader.init ? AppPreLoader.sampleRate : 48000, flag, IntPtr.Zero))
//#else
        if (!Bass.BASS_Init(-1, 48000, flag, IntPtr.Zero))
//#endif
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
                GameObject.Find("UIManager")?.GetComponent<UIManager>()?.OnAudioFinish(restart);
            }
        }
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

    public BassMemStream StreamSound(byte[] file, BASSFlag flags = BASSFlag.BASS_DEFAULT, float volume = 1f)
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
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_VOL, volume);
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

    private IEnumerator DelayPlayBGM(byte[] audioData, float seconds)
    {
        lastPos = int.MinValue;
        BGMStream = StreamSound(audioData, BASSFlag.BASS_STREAM_DECODE, LiveSetting.bgmVolume);
        BGMStream.Play();
        BGMStream.Pause();
        yield return new WaitUntil(() => SceneLoader.Loading == false);

        loading = true;
        lastPos = LiveSetting.audioOffset - (int)(seconds * 1000);
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

    public void DelayPlay(byte[] audioData, float seconds)
    {
        StartCoroutine(DelayPlayBGM(audioData, seconds));
    }

    public int GetBGMPlaybackTime()
    {
        if (loading)
        {
            lastPos += Mathf.RoundToInt(Time.deltaTime * 1000);
            return lastPos;
        }

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
            if (BGMStream.Status == BASSActive.BASS_ACTIVE_PAUSED)
            {
                return lastPos + LiveSetting.audioOffset;
            }      

            lastPos = time;
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
        return status == BASSActive.BASS_ACTIVE_PAUSED && !UIManager.BitingTheDust;
    }

    public void PauseBGM()
    {
        BGMStream?.Pause();
    }

    public void ResumeBGM()
    {
        BGMStream.Play();
    }

    public void StopBGM()
    {
        BGMStream?.Stop();
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