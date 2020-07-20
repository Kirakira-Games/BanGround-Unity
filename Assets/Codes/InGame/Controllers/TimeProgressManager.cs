﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioProvider;
using State = GameStateMachine.State;

public class TimeProgressManager : MonoBehaviour
{
    private Slider TimeProgress;
    private uint audioLength;
    private ISoundTrack gameBGM;

    void Start()
    {
        TimeProgress = GetComponent<Slider>();
        gameBGM = AudioManager.Instance.gameBGM;
    }

    void Update()
    {
        switch (UIManager.Instance.SM.Current)
        {
            case State.Loading:
                TimeProgress.value = 0;
                break;
            case State.Finished:
                TimeProgress.value = 1;
                break;
            default:
                TimeProgress.value = Mathf.Clamp01(AudioTimelineSync.instance.GetTimeInMs() / (float)gameBGM.GetLength());
                break;
        }
    }
}
