using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#pragma warning disable 0414

public class FPSCounter : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;

    private Text text;
    private int frameInSec = 0;
    private float lastClearTime = -1;
    private int lastFPS = 0;

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
        if (AudioTimelineSync.instance != null && audioManager.gameBGM != null)
        {
            uint audioTime = audioManager.gameBGM.GetPlaybackTime();
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
