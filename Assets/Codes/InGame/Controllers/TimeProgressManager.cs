using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioProvider;
using State = GameStateMachine.State;
using Zenject;

public class TimeProgressManager : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IAudioTimelineSync audioTimelineSync;
    [Inject]
    private IGameStateMachine SM;

    private Slider TimeProgress;
    private uint audioLength;
    private ISoundTrack gameBGM;

    void Start()
    {
        TimeProgress = GetComponent<Slider>();
        gameBGM = audioManager.gameBGM;
    }

    void Update()
    {
        if(gameBGM == null)
            gameBGM = audioManager.gameBGM;

        switch (SM.Current)
        {
            case State.Loading:
                TimeProgress.value = 0;
                break;
            case State.Finished:
                TimeProgress.value = 1;
                break;
            default:
                if (gameBGM == null)
                    return;
                TimeProgress.value = Mathf.Clamp01(audioTimelineSync.timeInMs / (float)gameBGM.GetLength());
                break;
        }
    }
}
