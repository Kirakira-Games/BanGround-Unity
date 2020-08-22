using UnityEngine;
using System.Collections;
using UniRx.Async;
using AudioProvider;
using System.Threading;

public interface IAudioManager
{
    IAudioProvider Provider { get; }
    ISoundTrack gameBGM { get; set; }
    UniTask<ISoundEffect> PrecacheSE(byte[] data);
    UniTask<ISoundEffect> PrecacheInGameSE(byte[] data);
    UniTaskVoid DelayPlayInGameBGM(IAudioTimelineSync audioTimelineSync, byte[] audio, float seconds, CancellationTokenSource cts = default);
    UniTask<ISoundTrack> PlayLoopMusic(byte[] audio, bool needLoop = true, uint[] times = null, bool noFade = true);
    void StopBGM();
    void StopAllCoroutines();
}
