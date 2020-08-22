using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioProvider;
using UniRx.Async;
using Zenject;

using State = GameStateMachine.State;
using System.Threading;

public class AudioManager : MonoBehaviour, IAudioManager
{
    [Inject]
    public IAudioProvider Provider { get; private set; }
    [Inject]
    private IModManager modManager;

    public ISoundTrack gameBGM { get; set; }

    [Inject(Id = "snd_bgm_volume")]
    KVar snd_bgm_volume;
    [Inject(Id = "snd_se_volume")]
    KVar snd_se_volume;
    [Inject(Id = "snd_igse_volume")]
    KVar snd_igse_volume;

    [Inject(Id = "snd_engine")]
    KVar snd_engine;
    [Inject(Id = "snd_buffer_bass")]
    KVar snd_buffer_bass;
    [Inject(Id = "snd_buffer_fmod")]
    KVar snd_buffer_fmod;


    private void Awake()
    {
        int bufferIndex;

        if (!AppPreLoader.init) return;

        if (snd_engine != "Unity")
        {
            // Disable Unity Audio
            var cfg = AudioSettings.GetConfiguration();
            cfg.dspBufferSize = 0;
            cfg.sampleRate = 0;
            cfg.numRealVoices = 0;
            cfg.numVirtualVoices = 0;
            cfg.speakerMode = AudioSpeakerMode.Stereo;

            AudioSettings.Reset(cfg);
        }

        if (snd_engine == "Bass") 
        {
            bufferIndex = snd_buffer_bass;
            if (bufferIndex == -1)
            {
                snd_buffer_bass.Set(5);
                bufferIndex = 5;
            }
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.BassBufferScale[bufferIndex]));
        }
        else if(snd_engine == "Fmod")
        {
            bufferIndex = snd_buffer_fmod;
            if (bufferIndex == -1)
            {
                snd_buffer_fmod.Set(2);
                bufferIndex = 2;
            }
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]));
        }
        else if(snd_engine == "Unity")
        {
            bufferIndex = snd_buffer_fmod;
            if (bufferIndex == -1)
            {
                snd_buffer_fmod.Set(2);
                bufferIndex = 2;
            }
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]));
        }

        Provider.SetSoundEffectVolume(snd_se_volume, SEType.Common);
        Provider.SetSoundEffectVolume(snd_igse_volume, SEType.InGame);
        Provider.SetSoundTrackVolume(snd_bgm_volume);
    }

    private void Update()
    {
        Provider.Update();
/*
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_STANDALONE)
        if (Input.GetKeyUp(KeyCode.Escape) && !exiting)
        {
            waiting += 1.5f;

            if (waiting > 1.5f)
            {
                exiting = true;
                SceneManager.LoadSceneAsync("GameOver", LoadSceneMode.Additive);
            }
            else
            {
                MessageBannerController.ShowMsg(LogLevel.INFO, "Tap Again to End The Game");
            }
        }

        waiting -= Time.deltaTime;
        if (waiting < 0) waiting = 0;
#endif*/

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

    public async UniTask<ISoundEffect> PrecacheSE(byte[] data) => await Provider.PrecacheSE(data, SEType.Common);
    public async UniTask<ISoundEffect> PrecacheInGameSE(byte[] data) => await Provider.PrecacheSE(data, SEType.InGame);
    public async UniTaskVoid DelayPlayInGameBGM(IAudioTimelineSync audioTimelineSync, byte[] audio, float seconds, CancellationTokenSource cts = default)
    {
        if(Provider is PureUnityAudioProvider)
        {
            gameBGM = await ((PureUnityAudioProvider)Provider).PrecacheTrack(audio);
        }
        else
        {
            gameBGM = await Provider.StreamTrack(audio);
        }
        
        gameBGM.Play();
        gameBGM.Pause();

        await UniTask.WaitUntil(() => SceneLoader.Loading == false, cancellationToken: cts.Token);

        audioTimelineSync.time = -seconds;
        audioTimelineSync.Play();

        await UniTask.WaitUntil(() => audioTimelineSync.time >= -0.02, cancellationToken: cts.Token);

        foreach (var mod in modManager.attachedMods)
        {
            if (mod is AudioMod)
                (mod as AudioMod).ApplyMod(gameBGM);
        }

        if (UIManager.Instance.SM.Count > 1)
            await UniTask.WaitUntil(() => UIManager.Instance.SM.Count == 1, cancellationToken: cts.Token);
        InGameBackground.instance.playVideo();
        gameBGM.Play();
        UIManager.Instance.SM.Transit(State.Loading, State.Playing);

        await UniTask.WaitUntil(() => gameBGM.GetPlaybackTime() > 0, cancellationToken: cts.Token);
        audioTimelineSync.timeInMs = (int)gameBGM.GetPlaybackTime();
    }
    public void StopBGM() => gameBGM.Stop();
    public async UniTask<ISoundTrack> PlayLoopMusic(byte[] audio,bool needLoop = true, uint[] times = null, bool noFade = true)
    {
        ISoundTrack soundTrack = await Provider.StreamTrack(audio);

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