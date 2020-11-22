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
        // Adjust canvas: TODO(GEEKiDoS)
        // 不用 DO 了
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
