using UnityEngine;
using System.Collections;

public class AudioTimelineSync : MonoBehaviour
{
    public static AudioTimelineSync instance;

    float startTime;
    float pauseTime;

    private void Awake()
    {
        instance = this;
        startTime = Time.time + 1e9f;
        pauseTime = -1e9f;
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
            return Time.realtimeSinceStartup - startTime;
        }
        return pauseTime;
    }

    public int GetTimeInMs()
    {
        return Mathf.RoundToInt(GetTimeInS() / 1000f);
    }

    public void Seek(float targetTime)
    {
        startTime = Time.realtimeSinceStartup - targetTime;
        if (!float.IsNaN(pauseTime))
        {
            pauseTime = targetTime;
        }
    }
}
