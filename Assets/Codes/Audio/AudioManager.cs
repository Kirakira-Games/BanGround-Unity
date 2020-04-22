using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AudioProvider;
using System.Threading.Tasks;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public static IAudioProvider Provider { get; private set; }

    public bool isInGame = false;
    public bool isLoading = false;
    //public bool restart = false;

    public ISoundTrack gameBGM { get; set; }

    private void Awake()
    {
        int bufferIndex;
        string engine = PlayerPrefs.GetString("AudioEngine", "Fmod");
        if (engine == "Bass") 
        {
            bufferIndex = PlayerPrefs.GetInt("BassBufferIndex", -1);
            if (bufferIndex == -1)
            {
                PlayerPrefs.SetInt("BassBufferIndex", 5);
                bufferIndex = 5;
            }
            Provider = new BassAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize * HandelValue_Buffer.BassBufferScale[bufferIndex]));
            //MessageBoxController.ShowMsg(LogLevel.INFO, $"Init {engine} with SampleRate: {AppPreLoader.sampleRate}, Buffer: {(int)(AppPreLoader.bufferSize * HandelValue_Buffer.BassBufferScale[bufferIndex])}");
        }
        else
        {
            bufferIndex = PlayerPrefs.GetInt("FmodBufferIndex", -1);
            if (bufferIndex == -1)
            {
                PlayerPrefs.SetInt("FmodBufferIndex", 2);
                bufferIndex = 2;
            }
            Provider = new FmodAudioProvider();
            Provider.Init(AppPreLoader.sampleRate, (uint)(AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]));
            //MessageBoxController.ShowMsg(LogLevel.INFO, $"Init {engine} with SampleRate: {AppPreLoader.sampleRate}, Buffer: {AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]}");
        }

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

        // ? 随便这里好像可以
        System.IO.KiraFilesystem.Instance.Dispose();
    }

    public ISoundEffect PrecacheSE(byte[] data) => Provider.PrecacheSE(data, SEType.Common);
    public ISoundEffect PrecacheInGameSE(byte[] data) => Provider.PrecacheSE(data, SEType.InGame);
    public void DelayPlayInGameBGM(byte[] audio, float seconds)
    {
        StartCoroutine(DelayPlayBGM(audio, seconds));
    }
    private IEnumerator DelayPlayBGM(byte[] audio, float seconds)
    {
        isLoading = true;
        gameBGM = Provider.StreamTrack(audio);
        gameBGM.Play();
        gameBGM.Pause();

        yield return new WaitUntil(() => SceneLoader.Loading == false);

        AudioTimelineSync.instance.Seek(-seconds);
        AudioTimelineSync.instance.Play();
        while (AudioTimelineSync.instance.GetTimeInS() < -0.05)
        {
            yield return new WaitForEndOfFrame();
        }

        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is AudioMod)
                (mod as AudioMod).ApplyMod(gameBGM);
        }

        InGameBackground.instance.playVideo();
        gameBGM.Play();
        isInGame = true;
        isLoading = false;

        while (gameBGM.GetPlaybackTime() == 0)
        {
            AudioTimelineSync.instance.Seek(0);
            yield return new WaitForEndOfFrame();
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