using UnityEngine;
using System.Collections;
using UniRx.Async;
using AudioProvider;

public interface IAudioManager
{
    IAudioProvider Provider { get; }
    ISoundTrack gameBGM { get; set; }
    UniTask<ISoundEffect> PrecacheSE(byte[] data);
    UniTask<ISoundEffect> PrecacheInGameSE(byte[] data);
    UniTask DelayPlayInGameBGM(byte[] audio, float seconds);
    UniTask<ISoundTrack> PlayLoopMusic(byte[] audio, bool needLoop = true, uint[] times = null, bool noFade = true);
    void StopBGM();
    void StopAllCoroutines();
}
