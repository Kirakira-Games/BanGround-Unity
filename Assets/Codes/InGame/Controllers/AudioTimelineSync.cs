using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using Zenject;
using AudioProvider;
using Cysharp.Threading.Tasks;
using System;

public class AudioTimelineSync : MonoBehaviour, IAudioTimelineSync
{
    private const int QUEUE_SIZE = 10;
    private const float DIFF_TOLERANCE = 0.1f;
    private const float UNSYNC_LIMIT = 10f;
    private const float SYNC_LEAD = 0.02f;

    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IModManager modManager;
    [Inject]
    private IInGameBackground inGameBackground;
    [Inject]
    private IGameStateMachine SM;
    [Inject]
    private ICancellationTokenStore Cancel;
    [Inject(Optional = true)]
    private IFPSCounter fpsCounter;

    private ISoundTrack bgm;
    private Stopwatch watch = new Stopwatch();
    private float deltaTime;

    // Sync display
    public float SmoothAudioDiff => syncQueue.Count == 0 ? float.NaN : totDiff / syncQueue.Count;
    private Queue<float> syncQueue = new Queue<float>();
    private float totDiff;
    private uint prevAudioTime;
    private float adjustLimit;

    public float BGMTimeToRealtime(float t)
    {
        return t / modManager.SpeedCompensationSum;
    }
    public int BGMTimeToRealtime(int t)
    {
        return Mathf.RoundToInt(t / modManager.SpeedCompensationSum);
    }

    public float RealTimeToBGMTime(float t)
    {
        return t * modManager.SpeedCompensationSum;
    }
    public int RealTimeToBGMTime(int t)
    {
        return Mathf.RoundToInt(t * modManager.SpeedCompensationSum);
    }

    private void ClearSync()
    {
        syncQueue.Clear();
        totDiff = 0;
        prevAudioTime = int.MaxValue;
    }

    private void Awake()
    {
        ClearSync();
        deltaTime = -1e3f;
        adjustLimit = UnityEngine.Time.smoothDeltaTime * 0.2f;
    }

    public void Play()
    {
        ClearSync();
        watch.Start();
    }

    public void Pause()
    {
        watch.Stop();
    }

    public float Time
    {
        get => (float)((watch.Elapsed.TotalSeconds + deltaTime) * modManager.SpeedCompensationSum);
        set { deltaTime = (float)(value / modManager.SpeedCompensationSum - watch.Elapsed.TotalSeconds); }
    }
    public int TimeInMs
    {
        get => Mathf.RoundToInt(Time * 1000);
        set { Time = value / 1000f; }
    }
    public float AudioSeekPos { get; set; }

    private async UniTaskVoid StartPlaying(ISoundTrack gameBGM)
    {
        inGameBackground.seekVideo(AudioSeekPos);
        gameBGM.SetPlaybackTime((uint)(AudioSeekPos * 1000));

        inGameBackground.playVideo();
        gameBGM.Play();
        SM.Transit(GameStateMachine.State.Loading, GameStateMachine.State.Playing);
        try
        {
            await UniTask.WaitUntil(() => gameBGM.GetPlaybackTime() > AudioSeekPos).WithCancellation(Cancel.sceneToken);
            TimeInMs = (int)gameBGM.GetPlaybackTime();
        }
        catch (OperationCanceledException) { }
    }

    private void UpdateSync()
    {
        if (SM.Base == GameStateMachine.State.Finished)
            return;
        float currentTime = Time;
        if (bgm.GetStatus() != PlaybackStatus.Playing)
        {
            if (currentTime >= AudioSeekPos - SYNC_LEAD && SM.Current == GameStateMachine.State.Loading)
            {
                StartPlaying(bgm).Forget();
            }
            return;
        }
        // Audio sync test
        uint audioTime = bgm.GetPlaybackTime();
        if (audioTime == prevAudioTime)
            return;
        prevAudioTime = audioTime;
        float diff = BGMTimeToRealtime(audioTime / 1000f - currentTime);
        // too unsync
        if (diff < -UNSYNC_LIMIT)
            return;

        syncQueue.Enqueue(diff);
        totDiff += diff;
        while (syncQueue.Count > QUEUE_SIZE)
        {
            totDiff -= syncQueue.Dequeue();
        }
        // Try to sync
        float adjust = diff * 0.2f;
        if (Mathf.Abs(diff) > DIFF_TOLERANCE)
        {
            adjust = diff;
        }
        else
        {
            if (Mathf.Abs(adjust) > adjustLimit)
                adjust = adjustLimit * Mathf.Sign(diff);
        }
        deltaTime += adjust;
    }

    private void Update()
    {
        if (bgm == null)
            bgm = audioManager.gameBGM;
        if (bgm == null)
            return;
        UpdateSync();

        if(!bgm.Disposed)
            fpsCounter?.AppendExtraInfo($"S: {bgm.GetPlaybackTime()}/{TimeInMs}");
    }
}
