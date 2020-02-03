using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FMOD;
using System.Runtime.InteropServices;

class AudioManager : MonoBehaviour
{
    FMOD.System System;

    ChannelGroup SEChannelGroup;
    ChannelGroup BGMChannelGroup;

    Channel CurrentBGMChannel;

    public List<Sound> LoadedSound = new List<Sound>();

    public bool loading = true;//bgm will not start untill the gate open
    public bool isInGame;

    private int lastPos = -1;
    private float lastUpdateTime = -1;

    void Awake()
    {
        Factory.System_Create(out System);
        System.init(1024, INITFLAGS.NORMAL, IntPtr.Zero);

        System.createChannelGroup("SoundEffects", out SEChannelGroup);
        System.createChannelGroup("BackgroundMuisc", out BGMChannelGroup);

        SEChannelGroup.setVolume(LiveSetting.seVolume);
        BGMChannelGroup.setVolume(LiveSetting.bgmVolume);
    }


    void Update()
    {
        System.update();
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

    public Sound PrecacheSound(TextAsset asset, MODE mode = MODE._2D | MODE.OPENMEMORY)
    {
        var bytes = asset.bytes;

        Sound sound;

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(exinfo);
        exinfo.length = (uint)bytes.Length;

        var result = System.createSound(bytes, mode, ref exinfo, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    public Sound PrecacheSound(byte[] bytes, MODE mode = MODE._2D | MODE.OPENMEMORY)
    {
        Sound sound;

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(exinfo);
        exinfo.length = (uint)bytes.Length;

        var result = System.createSound(bytes, mode, ref exinfo, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    public Sound PrecacheSound(string path, MODE mode = MODE._2D)
    {
        Sound sound;

        var result = System.createSound(path, mode, out sound);

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

    public Channel PlayBGM(Sound sound, bool paused = false)
    {
        
        Channel channel;
        
        System.playSound(sound, BGMChannelGroup, false, out channel);
        
        CurrentBGMChannel = channel;
        return channel;
    }

    public Channel PlayPreview(Sound sound)
    {
        Channel channel;
        sound.getLength(out uint length, TIMEUNIT.MS);

        sound.setLoopPoints(0, TIMEUNIT.MS, length, TIMEUNIT.MS);
        sound.setLoopCount(int.MaxValue);

        System.playSound(sound, BGMChannelGroup, false, out channel);

        CurrentBGMChannel = channel;
        return channel;
    }

    public int GetBGMPlaybackTime()
    {
        if (loading) return (int)(Time.time * 1000) - lastPos + LiveSetting.audioOffset;
        uint pos;
        CurrentBGMChannel.getPosition(out pos, TIMEUNIT.MS);

        if (GetPlayStatus())
        {
            if (pos != lastPos)
            {
                lastPos = (int)pos;
                lastUpdateTime = Time.time;
            }
            else
            {
                return (int)((Time.time - lastUpdateTime) * 1000) + lastPos + LiveSetting.audioOffset;
            }
        }

        return (int)pos + LiveSetting.audioOffset;
    }

    public bool GetPlayStatus()
    {
        CurrentBGMChannel.isPlaying(out bool isPlaying);
        return isPlaying;
    }

    public bool GetPauseStatus()
    {
        CurrentBGMChannel.getPaused(out bool paused);
        return paused;
    }

    public void PauseBGM()
    {
        print("pause");
        CurrentBGMChannel.setPaused(true);
    }

    public void ResumeBGM()
    {
        CurrentBGMChannel.setPaused(false);
    }

    public void StopBGM()
    {
        CurrentBGMChannel.stop();
    }

    void OnApplicationQuit()
    {
        foreach (var sound in LoadedSound)
            sound.release();

        SEChannelGroup.release();
        BGMChannelGroup.release();
        System.release();
    }

    private void OnDestroy()
    {
        foreach (var sound in LoadedSound)
            sound.release();

        SEChannelGroup.release();
        BGMChannelGroup.release();
        System.release();
    }
}