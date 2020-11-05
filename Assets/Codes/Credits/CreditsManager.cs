using AudioProvider;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class CreditsManager : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;

    public TextAsset bgmVoice;

    private ISoundTrack bgmST;

    async void Start()
    {
        bgmST = await audioManager.PlayLoopMusic(bgmVoice.bytes);
    }

    public void ExitCreditScene()
    {
        SceneLoader.LoadScene("Credits", "Title");
    }

    private void OnDestroy()
    {
        bgmST.Dispose();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) bgmST?.Play();
        else bgmST?.Pause();
    }
}
