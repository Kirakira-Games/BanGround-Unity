using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioProvider;
using UniRx.Async;

using State = GameStateMachine.State;

public class AudioManager : MonoBehaviour, IAudioManager
{
    public IAudioProvider Provider { get; private set; }

    public ISoundTrack gameBGM { get; set; }

    static KVar snd_bgm_volume = new KVar("snd_bgm_volume", "0.7", KVarFlags.Archive, "BGM volume");
    static KVar snd_se_volume = new KVar("snd_se_volume", "0.7", KVarFlags.Archive, "Sound effect volume");
    static KVar snd_igse_volume = new KVar("snd_igse_volume", "0.7", KVarFlags.Archive, "In-game sound effect volume");

    private void Awake()
    {
        KVar snd_engine = new KVar("snd_engine", "Fmod", KVarFlags.Archive | KVarFlags.StringOnly, "Sound engine type");
        KVar snd_buffer_bass = new KVar("snd_buffer_bass", "-1", KVarFlags.Archive, "Buffer type of Bass Sound Engine");
        KVar snd_buffer_fmod = new KVar("snd_buffer_fmod", "-1", KVarFlags.Archive, "Buffer size of Fmod/Unity Sound Engine");

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
            Provider = new BassAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize * HandelValue_Buffer.BassBufferScale[bufferIndex]));
        }
        else if(snd_engine == "Fmod")
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
        else if(snd_engine == "Unity")
        {
            bufferIndex = snd_buffer_fmod;
            if (bufferIndex == -1)
            {
                snd_buffer_fmod.Set(2);
                bufferIndex = 2;
            }
            Provider = new PureUnityAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]));
        }

        Provider.SetSoundEffectVolume(snd_se_volume, SEType.Common);
        Provider.SetSoundEffectVolume(snd_igse_volume, SEType.InGame);
        Provider.SetSoundTrackVolume(snd_bgm_volume);

        DontDestroyOnLoad(gameObject);
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
    public async UniTask DelayPlayInGameBGM(byte[] audio, float seconds)
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

        await UniTask.WaitUntil(() => SceneLoader.Loading == false, cancellationToken: UIManager.Instance.cancellationToken.Token);

        AudioTimelineSync.instance.Seek(-seconds);
        AudioTimelineSync.instance.Play();

        await UniTask.WaitUntil(() => AudioTimelineSync.instance.GetTimeInS() >= -0.02, cancellationToken: UIManager.Instance.cancellationToken.Token);

        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is AudioMod)
                (mod as AudioMod).ApplyMod(gameBGM);
        }

        if (UIManager.Instance.SM.Count > 1)
            await UniTask.WaitUntil(() => UIManager.Instance.SM.Count == 1, cancellationToken: UIManager.Instance.cancellationToken.Token);
        InGameBackground.instance.playVideo();
        gameBGM.Play();
        UIManager.Instance.SM.Transit(State.Loading, State.Playing);

        await UniTask.WaitUntil(() => gameBGM.GetPlaybackTime() > 0, cancellationToken: UIManager.Instance.cancellationToken.Token);
        AudioTimelineSync.instance.Seek(gameBGM.GetPlaybackTime() / 1000f);
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