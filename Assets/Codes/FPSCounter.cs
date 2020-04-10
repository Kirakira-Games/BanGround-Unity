using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    Text text;
    int frameInSec = 0;
    int freamTimeInSec = 0;
    float lastClearTime = -1;
    int lastFPS = 0;

    // Sync display
    int audioSyncDiff;
    Queue<int> syncQueue;
    int totDiff;
    private const int audioSyncCount = 30;

    float lostFocusTime = 0;

    void ClearSync()
    {
        syncQueue.Clear();
        totDiff = 0;
        audioSyncDiff = int.MinValue;
    }

    void Awake()
    {
        syncQueue = new Queue<int>();
        text = GetComponent<Text>();
        ClearSync();
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
        // Audio sync test
        int? audioTime = (int?)AudioManager.Instance?.gameBGM?.GetPlaybackTime();
        int? syncTime = AudioTimelineSync.instance?.GetTimeInMs();
        if (audioTime.HasValue && syncTime.HasValue)
        {
            int diff = syncTime.Value - audioTime.Value;
            syncQueue.Enqueue(diff);
            totDiff += diff;
            while (syncQueue.Count > audioSyncCount)
            {
                totDiff -= syncQueue.Dequeue();
            }
            audioSyncDiff = totDiff / syncQueue.Count;
        }
        else
        {
            ClearSync();
        }
        if (audioSyncDiff != int.MinValue)
        {
            str += $"\nSync: {(audioSyncDiff >= 0 ? "+" + audioSyncDiff : audioSyncDiff.ToString())}ms";
        }
        text.text = str;
    }
}
