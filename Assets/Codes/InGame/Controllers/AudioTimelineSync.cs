﻿using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Zenject;
using AudioProvider;
using UniRx.Async;

public class AudioTimelineSync : MonoBehaviour, IAudioTimelineSync
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IModManager modManager;
    [Inject]
    private IInGameBackground inGameBackground;
    [Inject]
    private IGameStateMachine SM;

    private Stopwatch watch = new Stopwatch();
    private float deltaTime;

    // Sync display
    public float smoothAudioDiff => syncQueue.Count == 0 ? float.NaN : totDiff / syncQueue.Count;
    private Queue<float> syncQueue = new Queue<float>();
    private const int QUEUE_SIZE = 10;
    private const float DIFF_TOLERANCE = 0.1f;
    private const float UNSYNC_LIMIT = 10f;
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
        adjustLimit = Time.smoothDeltaTime * 0.2f;
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

    public float time
    {
        get => (float)((watch.Elapsed.TotalSeconds + deltaTime) * modManager.SpeedCompensationSum);
        set { deltaTime = (float)(value / modManager.SpeedCompensationSum - watch.Elapsed.TotalSeconds); }
    }
    public int timeInMs
    {
        get => Mathf.RoundToInt(time * 1000);
        set { time = value / 1000f; }
    }

    private async UniTaskVoid StartPlaying(ISoundTrack gameBGM)
    {
        inGameBackground.playVideo();
        gameBGM.Play();
        SM.Transit(GameStateMachine.State.Loading, GameStateMachine.State.Playing);

        await UniTask.WaitUntil(() => gameBGM.GetPlaybackTime() > 0); // TODO: cancellationToken: cts.Token;
        timeInMs = (int)gameBGM.GetPlaybackTime();
    }

    private void Update()
    {
        var bgm = audioManager.gameBGM;
        if (SM.Base == GameStateMachine.State.Finished || bgm == null)
            return;
        float currentTime = time;
        if (bgm.GetStatus() != PlaybackStatus.Playing)
        {
            if (currentTime >= -0.02 && SM.Current == GameStateMachine.State.Loading)
            {
                _ = StartPlaying(bgm);
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
}
