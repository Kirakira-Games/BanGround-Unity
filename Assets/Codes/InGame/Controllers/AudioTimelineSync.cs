using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;

public class AudioTimelineSync : MonoBehaviour
{
    public static AudioTimelineSync instance;

    private Stopwatch watch;
    private float deltaTime;

    // Sync display
    public float smoothAudioDiff => syncQueue.Count == 0 ? float.NaN : totDiff / syncQueue.Count;
    private Queue<float> syncQueue;
    private const int QUEUE_SIZE = 10;
    private float totDiff;
    private uint prevAudioTime;
    private static float adjustLimit;

    public static float BGMTimeToRealtime(float t)
    {
        return t / LiveSetting.SpeedCompensationSum;
    }
    public static int BGMTimeToRealtime(int t)
    {
        return Mathf.RoundToInt(t / LiveSetting.SpeedCompensationSum);
    }

    public static float RealTimeToBGMTime(float t)
    {
        return t * LiveSetting.SpeedCompensationSum;
    }
    public static int RealTimeToBGMTime(int t)
    {
        return Mathf.RoundToInt(t * LiveSetting.SpeedCompensationSum);
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
        adjustLimit = Time.deltaTime * 0.2f * LiveSetting.SpeedCompensationSum;
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
        return (float)((watch.Elapsed.TotalSeconds + deltaTime) * LiveSetting.SpeedCompensationSum);
    }

    public int GetTimeInMs()
    {
        return Mathf.RoundToInt(GetTimeInS() * 1000);
    }

    public void Seek(float targetTime)
    {
        deltaTime = (float)(targetTime / LiveSetting.SpeedCompensationSum - watch.Elapsed.TotalSeconds);
    }

    private void Update()
    {
        var bgm = AudioManager.Instance?.gameBGM;
        if (bgm == null || bgm.GetStatus() != AudioProvider.PlaybackStatus.Playing)
            return;
        // Audio sync test
        uint audioTime = bgm.GetPlaybackTime();
        if (audioTime == prevAudioTime)
            return;
        prevAudioTime = audioTime;
        float diff = audioTime / 1000f - GetTimeInS();
        syncQueue.Enqueue(diff);
        totDiff += diff;
        while (syncQueue.Count > QUEUE_SIZE)
        {
            totDiff -= syncQueue.Dequeue();
        }
        // Try to sync
        float adjust = diff * 0.2f;
        if (Mathf.Abs(adjust) > adjustLimit)
            adjust = adjustLimit * Mathf.Sign(diff);
        deltaTime += adjust;
    }
}
