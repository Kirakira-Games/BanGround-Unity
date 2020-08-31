using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx.Async;
using XLua;

namespace AudioProvider
{
    public enum PlaybackStatus
    {
        Stopped = 0,
        Paused,
        Playing,
        Unknown = -1
    }
    public enum SEType
    {
        Common = 0,
        InGame,

        Unknown = -1
    }

    public interface ISoundTrack : IDisposable
    {
        bool Disposed { get; }

        void Play();
        void Pause();
        [Obsolete]
        void Resume();
        void Stop();
        void Restart();

        void SetLoopingPoint(uint start, uint end, bool noFade);
        void SetTimeScale(float scale, bool noPitchShift);
        void SetPlaybackTime(uint time);
        void SetVolume(float volume);

        uint GetPlaybackTime();
        uint GetLength();
        PlaybackStatus GetStatus();
    }

    public interface ISoundEffect : IDisposable
    {
        bool Disposed { get; }

        void PlayOneShot();
    }

    public interface IAudioProvider
    {
        void Init(int sampleRate, uint bufferLength);
        void Unload();
        void Update();

        void SetMasterVolume(float volume);
        void SetSoundTrackVolume(float volume);
        void SetSoundEffectVolume(float volume, SEType type);

        UniTask<ISoundEffect> PrecacheSE(byte[] audio, SEType type);
        UniTask<ISoundTrack> StreamTrack(byte[] audio);
    }
}
