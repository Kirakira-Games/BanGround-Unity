using AudioProvider;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Canvas))]
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
        SceneLoader.Back(null);
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
