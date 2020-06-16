using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioProvider;

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
        if (AudioManager.Instance.isInGame)
        {
            TimeProgress.value = gameBGM.GetPlaybackTime() / (float)gameBGM.GetLength();
        }
    }
}
