using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0414

public class FPSCounter : MonoBehaviour
{
    Text text;
    int frameInSec = 0;
    int freamTimeInSec = 0;
    float lastClearTime = -1;
    int lastFPS = 0;

    float lostFocusTime = 0;

    void Awake()
    {
        text = GetComponent<Text>();
    }
    void Update()
    {
        if (Time.time - lastClearTime > 1)
        {
            lastFPS = frameInSec;
            frameInSec = 0;
            lastClearTime = Time.time;
        }

        if (Time.timeScale == 0) return;
        frameInSec++;

        string str = $"FPS : {lastFPS}";

        // Audio diff display
        if (AudioTimelineSync.instance != null && AudioManager.Instance.gameBGM != null)
        {
            uint audioTime = AudioManager.Instance.gameBGM.GetPlaybackTime();
            int syncTime = AudioTimelineSync.instance.GetTimeInMs();
            str += $"\nS: {audioTime}/{syncTime}";
            /*
            float? audioDiff = AudioTimelineSync.instance?.smoothAudioDiff;
            if (audioDiff.HasValue && !float.IsNaN(audioDiff.Value))
            {
                int diff = Mathf.RoundToInt(audioDiff.Value * 1000);
                string diffStr = diff >= 0 ? "+" + diff : diff.ToString();
                str += $"\nSync: {diffStr}ms";
            }
            */
        }

        text.text = str;
    }
}
