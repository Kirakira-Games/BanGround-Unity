using System;
using System.Collections.Generic;

using UnityEngine;
using FMOD;
using System.Runtime.InteropServices;

class AudioManager : MonoBehaviour
{
    FMOD.System System;

    ChannelGroup SEChannelGroup;
    ChannelGroup BGMChannelGroup;

    List<Sound> LoadedSound = new List<Sound>();

    void Awake()
    {
        Factory.System_Create(out System);
        var result = System.init(64, INITFLAGS.NORMAL, IntPtr.Zero);

        result = System.createChannelGroup("SoundEffects", out SEChannelGroup);

        result = System.createChannelGroup("BackgroundMuisc", out BGMChannelGroup);
    }

    Sound PrecacheSound(TextAsset asset)
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

    Sound PrecacheSound(byte[] bytes)
    {
        Sound sound;

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(exinfo);
        exinfo.length = (uint)bytes.Length;

        var result = System.createSound(bytes, MODE._2D | MODE.OPENMEMORY, ref exinfo, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    Sound PrecacheSound(string path)
    {
        Sound sound;

        var result = System.createSound(path, MODE._2D, out sound);

        LoadedSound.Add(sound);

        return sound;
    }

    void UnloadSound(Sound sound)
    {
        LoadedSound.Remove(sound);
        sound.release();
    }

    Channel PlaySE(Sound sound)
    {
        Channel channel = new Channel();
        channel.setChannelGroup(SEChannelGroup);

        System.playSound(sound, SEChannelGroup, false, out channel);

        return channel;
    }

    Channel PlayBGM(Sound sound)
    {
        Channel channel = new Channel();
        channel.setChannelGroup(BGMChannelGroup);

        System.playSound(sound, BGMChannelGroup, false, out channel);

        return channel;
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