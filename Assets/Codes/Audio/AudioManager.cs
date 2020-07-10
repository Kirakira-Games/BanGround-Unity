using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioProvider;
using UniRx.Async;

using State = GameStateMachine.State;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public static IAudioProvider Provider { get; private set; }

    public ISoundTrack gameBGM { get; set; }

    static KVar snd_bgm_volume = new KVar("snd_bgm_volume", "0.7", KVarFlags.Archive, "BGM volume");
    static KVar snd_se_volume = new KVar("snd_se_volume", "0.7", KVarFlags.Archive, "Sound effect volume");
    static KVar snd_igse_volume = new KVar("snd_igse_volume", "0.7", KVarFlags.Archive, "In-game sound effect volume");

    private void Awake()
    {
        KVar snd_engine = new KVar("snd_engine", "Fmod", KVarFlags.Archive | KVarFlags.StringOnly, "Sound engine type");
        KVar snd_buffer_bass = new KVar("snd_buffer_bass", "-1", KVarFlags.Archive, "Buffer size of Bass Sound Engine");
        KVar snd_buffer_fmod = new KVar("snd_buffer_fmod", "-1", KVarFlags.Archive, "Buffer size of Fmod Sound Engine");
        int bufferIndex;
        //string engine = PlayerPrefs.GetString("AudioEngine", "Fmod");
        if (snd_engine == "Bass") 
        {
            bufferIndex = snd_buffer_bass;
            if (bufferIndex == -1)
            {
                snd_buffer_bass.Set(5);
                bufferIndex = 5;
            }
            Provider = new BassAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize * HandelValue_Buffer.BassBufferScale[bufferIndex]));
        }
        else
        {
            bufferIndex = snd_buffer_fmod;
            if (bufferIndex == -1)
            {
                snd_buffer_fmod.Set(2);
                bufferIndex = 2;
            }
            Provider = new FmodAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]));
        }

        Provider.SetSoundEffectVolume(snd_se_volume, SEType.Common);
        Provider.SetSoundEffectVolume(snd_igse_volume, SEType.InGame);
        Provider.SetSoundTrackVolume(snd_bgm_volume);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Provider.Update();
    }

    private void OnDestroy()
    {
        Provider.Unload();

        try
        {
            // ? 随便这里好像可以
            System.IO.KiraFilesystem.Instance.Dispose();
        }
        catch
        {

        }
    }

    public ISoundEffect PrecacheSE(byte[] data) => Provider.PrecacheSE(data, SEType.Common);
    public ISoundEffect PrecacheInGameSE(byte[] data) => Provider.PrecacheSE(data, SEType.InGame);
    public async UniTaskVoid DelayPlayInGameBGM(byte[] audio, float seconds)
    {
        gameBGM = Provider.StreamTrack(audio);
        gameBGM.Play();
        gameBGM.Pause();

        await UniTask.WaitUntil(() => SceneLoader.Loading == false);

        AudioTimelineSync.instance.Seek(-seconds);
        AudioTimelineSync.instance.Play();
        while (AudioTimelineSync.instance.GetTimeInS() < -0.02)
        {
            await UniTask.DelayFrame(1);
        }

        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is AudioMod)
                (mod as AudioMod).ApplyMod(gameBGM);
        }

        if (UIManager.Instance.SM.Current != State.Loading)
            await UniTask.WaitUntil(() => UIManager.Instance.SM.Current == State.Loading);
        InGameBackground.instance.playVideo();
        gameBGM.Play();
        UIManager.Instance.SM.Transit(State.Loading, State.Playing);

        while (gameBGM.GetPlaybackTime() == 0)
        {
            AudioTimelineSync.instance.Seek(0);
            await UniTask.DelayFrame(1);
        }
        AudioTimelineSync.instance.Seek(gameBGM.GetPlaybackTime() / 1000f);
    }
    public void StopBGM() => gameBGM.Stop();
    public ISoundTrack PlayLoopMusic(byte[] audio,bool needLoop = true, uint[] times = null, bool noFade = true)
    {
        ISoundTrack soundTrack = Provider.StreamTrack(audio);

        uint start = 0;
        uint end = soundTrack.GetLength();
        if (times != null)
        {
            start = times[0] < 0 ? 0 :times[0];
            end = times[1] <= 0 ? soundTrack.GetLength() : times[1];
        }

        if (needLoop)
            soundTrack.SetLoopingPoint(start, end, noFade);
        soundTrack.Play();
        return soundTrack;
    }
}