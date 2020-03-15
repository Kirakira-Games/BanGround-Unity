using UnityEngine;
using System.Collections;

public class AudioTimelineSync : MonoBehaviour
{
    public static AudioTimelineSync instance;

    float startTime;
    float pauseTime;

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
        return RealTimeToBGMTime(t - startTime);
    }

    private void Awake()
    {
        instance = this;
        startTime = Time.realtimeSinceStartup + 1e3f;
        pauseTime = -1e3f;
    }

    public void Play()
    {
        pauseTime = float.NaN;
    }

    public void Pause()
    {
        pauseTime = GetTimeInS();
    }

    public float GetTimeInS()
    {
        if (float.IsNaN(pauseTime))
        {
            return RealTimeToBGMTime(Time.realtimeSinceStartup - startTime);
        }
        return pauseTime;
    }

    public int GetTimeInMs()
    {
        return Mathf.RoundToInt(GetTimeInS() * 1000f);
    }

    public void Seek(float targetTime)
    {
        startTime = Time.realtimeSinceStartup - BGMTimeToRealtime(targetTime);
        if (!float.IsNaN(pauseTime))
        {
            pauseTime = targetTime;
        }
    }
}
