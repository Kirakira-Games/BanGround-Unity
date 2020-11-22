using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioProvider;
using Cysharp.Threading.Tasks;
using Zenject;
using System.Linq;

public class AudioManager : MonoBehaviour, IAudioManager
{
    [Inject]
    public IAudioProvider Provider { get; private set; }

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

    Texture2D fftTex;

    private void Start()
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
    }

    float _peakValue = 0.0f;

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
                        messageBannerController.ShowMsg(LogLevel.INFO, "Tap Again to End The Game");
                    }
                }

                waiting -= Time.deltaTime;
                if (waiting < 0) waiting = 0;
        #endif*/

        /*
        var fftData = Provider.GetFFTData();

        if (fftData.Length == 0)
            return;

        // Normalize
        _peakValue = _peakValue * 0.99f + fftData.Max() * 0.01f;
        fftData = fftData.Select(flt => flt / _peakValue).ToArray();

        if (fftTex == null)
            fftTex = new Texture2D(fftData.Length / 2, 1, TextureFormat.RFloat, false);

        for(int i = 0; i < fftData.Length / 2; i++)
        {
            fftTex.SetPixel(i, 1, new Color(fftData[i], 0, 0));
        }

        fftTex.Apply();
        */
    }

    private void OnDestroy()
    {
        Provider.Unload();
    }

    public async UniTask<ISoundEffect> PrecacheSE(byte[] data) => await Provider.PrecacheSE(data, SEType.Common);
    public async UniTask<ISoundEffect> PrecacheInGameSE(byte[] data) => await Provider.PrecacheSE(data, SEType.InGame);
    public async UniTask<ISoundTrack> StreamGameBGMTrack(byte[] data)
    {
        /*if (Provider is PureUnityAudioProvider)
        {
            gameBGM = await ((PureUnityAudioProvider)Provider).PrecacheTrack(data);
        }
        else*/
        {
            gameBGM = await Provider.StreamTrack(data);
        }
        return gameBGM;
    }
    public void StopBGM() => gameBGM?.Stop();
    public async UniTask<ISoundTrack> PlayLoopMusic(byte[] audio,bool needLoop = true, uint[] times = null, bool noFade = true)
    {
        ISoundTrack soundTrack = await Provider.StreamTrack(audio);

        uint start = 0;
        uint end = soundTrack.GetLength();
        if (times != null)
        {
            times[0] = Math.Max(start, times[0]);
            times[1] = Math.Min(end, times[1]);
            if (times[1] >= times[0] + 1000)
            {
                (start, end) = (times[0], times[1]);
            }
        }

        if (needLoop)
            soundTrack.SetLoopingPoint(start, end, noFade);

        soundTrack.Play();
        return soundTrack;
    }

    public Texture2D GetFFTTexture()
    {
        return fftTex;
    }
}