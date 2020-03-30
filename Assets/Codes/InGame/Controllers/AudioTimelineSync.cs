using UnityEngine;
using System.Diagnostics;
using System;

public class AudioTimelineSync : MonoBehaviour
{
    public static AudioTimelineSync instance;

    private Stopwatch watch;
    private float deltaTime;

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

    private void Awake()
    {
        instance = this;
        watch = new Stopwatch();
        deltaTime = -1e3f;
    }

    public void Play()
    {
        watch.Start();
    }

    public void Pause()
    {
        watch.Stop();
    }

    public float GetTimeInS()
    {
        return (float)(watch.Elapsed.TotalSeconds + deltaTime);
    }

    public int GetTimeInMs()
    {
        return Mathf.RoundToInt(GetTimeInS() * 1000);
    }

    public void Seek(float targetTime)
    {
        deltaTime = (float)(targetTime - watch.Elapsed.TotalSeconds);
    }
}
