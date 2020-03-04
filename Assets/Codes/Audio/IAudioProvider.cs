using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioProvider
{
    public enum PlaybackStatus
    {
        Stopped = 0,
        Paused,
        Playing,
        Unknown = -1
    }

    public interface ISoundTrack : IDisposable
    {
        void Play();
        void Pause();
        void Stop();
        void Restart();

        void SetLoopingPoint(uint start, uint end, bool noFade);
        void SetTimeScale(float scale, bool noPitchShift);
        void SetPlaybackTime(uint time);

        uint GetPlaybackTime();
        uint GetLength();
        PlaybackStatus GetStatus();
    }

    public interface ISoundEffect : IDisposable
    {
        void PlayOneShot();
    }

    public interface IAudioProvider
    {
        void Init(int sampleRate, uint bufferLength);
        void Unload();
        void Update();

        void SetMasterVolume(float volume);
        void SetSoundTrackVolume(float volume);
        void SetSoundEffectVolume(float volume);

        ISoundEffect PrecacheSE(byte[] audio);
        ISoundTrack StreamTrack(byte[] audio);
    }
}
