using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using AudioProvider;
using System.Threading;

public interface IAudioManager
{
    IAudioProvider Provider { get; }
    ISoundTrack gameBGM { get; set; }
    UniTask<ISoundEffect> PrecacheSE(byte[] data);
    UniTask<ISoundEffect> PrecacheInGameSE(byte[] data);
    UniTask<ISoundTrack> StreamGameBGMTrack(byte[] data);
    UniTask<ISoundTrack> PlayLoopMusic(byte[] audio, bool needLoop = true, uint[] times = null, bool noFade = true);
    void StopBGM();
    void StopAllCoroutines();
}
