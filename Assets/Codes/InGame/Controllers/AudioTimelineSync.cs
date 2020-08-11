using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Zenject;

public class AudioTimelineSync : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private ILiveSetting liveSetting;

    public static AudioTimelineSync instance;

    private Stopwatch watch;
    private float deltaTime;

    // Sync display
    public float smoothAudioDiff => syncQueue.Count == 0 ? float.NaN : totDiff / syncQueue.Count;
    private Queue<float> syncQueue;
    private const int QUEUE_SIZE = 10;
    private const float DIFF_TOLERANCE = 0.1f;
    private const float UNSYNC_LIMIT = 10f;
    private float totDiff;
    private uint prevAudioTime;
    private static float adjustLimit;

    public static float BGMTimeToRealtime(float t)
    {
        return t / LiveSetting.Instance.SpeedCompensationSum;
    }
    public static int BGMTimeToRealtime(int t)
    {
        return Mathf.RoundToInt(t / LiveSetting.Instance.SpeedCompensationSum);
    }

    public static float RealTimeToBGMTime(float t)
    {
        return t * LiveSetting.Instance.SpeedCompensationSum;
    }
    public static int RealTimeToBGMTime(int t)
    {
        return Mathf.RoundToInt(t * LiveSetting.Instance.SpeedCompensationSum);
    }

    public float TimeSinceStartupToBGMTime(float t)
    {
        throw new NotImplementedException();
    }

    private void ClearSync()
    {
        syncQueue.Clear();
        totDiff = 0;
        prevAudioTime = int.MaxValue;
    }

    private void Awake()
    {
        instance = this;
        watch = new Stopwatch();
        syncQueue = new Queue<float>();
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

    public float GetTimeInS()
    {
        return (float)((watch.Elapsed.TotalSeconds + deltaTime) * liveSetting.SpeedCompensationSum);
    }

    public int GetTimeInMs()
    {
        return Mathf.RoundToInt(GetTimeInS() * 1000);
    }

    public void Seek(float targetTime)
    {
        deltaTime = (float)(targetTime / liveSetting.SpeedCompensationSum - watch.Elapsed.TotalSeconds);
    }

    private void Update()
    {
        if (UIManager.Instance.SM.Base == GameStateMachine.State.Finished)
            return;
        var bgm = audioManager.gameBGM;
        if (bgm == null || bgm.GetStatus() != AudioProvider.PlaybackStatus.Playing)
            return;
        // Audio sync test
        uint audioTime = bgm.GetPlaybackTime();
        if (audioTime == prevAudioTime)
            return;
        prevAudioTime = audioTime;
        float diff = BGMTimeToRealtime(audioTime / 1000f - GetTimeInS());
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
