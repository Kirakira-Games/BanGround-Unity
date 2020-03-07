using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using ManagedBass.Fx;

namespace AudioProvider
{
    public class BassSoundTrack : ISoundTrack
    {
        int _internalChannelID;
        byte[] _audioInstanse;
        BassAudioProvider _provider;

        bool _isLooping, noFade;
        long _loopstart, _loopend, _loopendms;

        float _volume = 1.0f;

        internal BassSoundTrack(int id, byte[] audio, BassAudioProvider provider)
        {
            _internalChannelID = id;
            _audioInstanse = audio;
            _provider = provider;
        }

        public uint GetLength()
        {
            var b = Bass.ChannelGetLength(_internalChannelID, PositionFlags.Bytes);
            var s = Bass.ChannelBytes2Seconds(_internalChannelID, b);
            var ms = s * 1000;

            return (uint)ms;
        }

        public uint GetPlaybackTime()
        {
            var b = Bass.ChannelGetPosition(_internalChannelID, PositionFlags.Bytes);
            var s = Bass.ChannelBytes2Seconds(_internalChannelID, b);
            var ms = s * 1000;

            return (uint)ms;
        }

        public PlaybackStatus GetStatus()
        {
            var status = Bass.ChannelIsActive(_internalChannelID);

            switch(status)
            {
                case PlaybackState.Paused:
                    return PlaybackStatus.Paused;
                case PlaybackState.Playing:
                    return PlaybackStatus.Playing;
                default:
                    return PlaybackStatus.Stopped;
            }
        }

        public void Pause()
        {
            Bass.ChannelPause(_internalChannelID);
        }

        public void Play()
        {
            Bass.ChannelPlay(_internalChannelID, false);
        }

        public void Restart()
        {
            if (_isLooping)
                Bass.ChannelSetPosition(_internalChannelID, _loopstart);
            else
                Bass.ChannelSetPosition(_internalChannelID, 0);

            Play();
        }

        [Obsolete]
        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void SetLoopingPoint(uint start, uint end, bool noFade)
        {
            _loopstart = Bass.ChannelSeconds2Bytes(_internalChannelID, start / 1000.0);
            _loopend = Bass.ChannelSeconds2Bytes(_internalChannelID, end / 1000.0);
            _loopendms = end;
            _isLooping = true;
            this.noFade = noFade;

            Bass.ChannelSetPosition(_internalChannelID, _loopstart);
        }

        public void SetPlaybackTime(uint time)
        {
            var b = Bass.ChannelSeconds2Bytes(_internalChannelID, time / 1000.0);
            Bass.ChannelSetPosition(_internalChannelID, b);
        }

        public void SetTimeScale(float scale, bool noPitchShift)
        {
            var info = Bass.ChannelGetInfo(_internalChannelID);

            Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.Tempo, 0);
            Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.TempoFrequency, info.Frequency);

            if(noPitchShift)
            {
                var variable = (scale - 1) * 100;
                Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.Tempo, variable);
            }
            else
            {
                Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.TempoFrequency, info.Frequency * scale);
            }
        }

        public void SetVolume(float volume)
        {
            this._volume = volume;
            VolumeChanged();
        }

        public void Stop()
        {
            Bass.ChannelStop(_internalChannelID);
        }

        public void Dispose()
        {
            Stop();
            Bass.StreamFree(_internalChannelID);

            _provider.OnUnload -= Dispose;
            _provider.OnUpdate -= Update;
            _provider.OnVolumeChanged -= VolumeChanged;
        }

        internal void Update()
        {
            if (!_isLooping || GetStatus() != PlaybackStatus.Playing)
                return;

            long diff = _loopendms - GetPlaybackTime();

            if (diff < 2000)
            {
                if(!noFade)
                {
                    float volume = Math.Max(diff, 0) / 2000.0f;
                    Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.Volume, volume * _volume * _provider.trackVolume * _provider.masterVolume);
                }
                

                if (diff < -750)
                {
                    if (!noFade)
                        VolumeChanged();

                    Bass.ChannelSetPosition(_internalChannelID, _loopstart);
                }
            }
        }

        internal void VolumeChanged()
        {
            Bass.ChannelSetAttribute(_internalChannelID, ChannelAttribute.Volume, _volume * _provider.trackVolume * _provider.masterVolume);
        }
    }

    public class BassSoundEffect : ISoundEffect
    {
        int _internalSound;
        byte[] _byteInstanse;
        BassAudioProvider _provider;

        internal BassSoundEffect(int sound, byte[] audio, BassAudioProvider provider)
        {
            _internalSound = sound;
            _byteInstanse = audio;
            _provider = provider;
        }

        public void Dispose()
        {
            Bass.SampleFree(_internalSound);

            _provider.OnUnload -= Dispose;
        }

        public void PlayOneShot()
        {
            var channel = Bass.SampleGetChannel(_internalSound);
            Bass.ChannelSetAttribute(channel, ChannelAttribute.Volume, _provider.effectVolume * _provider.masterVolume);
            Bass.ChannelPlay(channel);
        }
    }

    public class BassAudioProvider : IAudioProvider
    {
        internal float trackVolume = 1.0f;
        internal float effectVolume = 1.0f;
        internal float masterVolume = 1.0f;

        internal delegate void BassEventHandler();
        internal event BassEventHandler OnUpdate;
        internal event BassEventHandler OnVolumeChanged;
        internal event BassEventHandler OnUnload;

        public void Init(int sampleRate, uint bufferLength)
        {
            Bass.Init(-1, sampleRate);

            if (bufferLength != 0) 
                Bass.Configure(Configuration.MixerBufferLength, (int)bufferLength);
        }          

        public ISoundTrack StreamTrack(byte[] audio)
        {
            var id = Bass.CreateStream(audio, 0, audio.Length, BassFlags.Decode | BassFlags.Prescan);
            var fxid = BassFx.TempoCreate(id, BassFlags.FxFreeSource);

            Bass.ChannelSetAttribute(id, ChannelAttribute.TempoUseQuickAlgorithm, 1);
            Bass.ChannelSetAttribute(id, ChannelAttribute.TempoOverlapMilliseconds, 4);
            Bass.ChannelSetAttribute(id, ChannelAttribute.TempoSequenceMilliseconds, 30);

            var st = new BassSoundTrack(fxid, audio, this);

            OnUpdate += st.Update;
            OnVolumeChanged += st.VolumeChanged;
            OnUnload += st.Dispose;

            st.VolumeChanged();

            return st;
        }

        public ISoundEffect PrecacheSE(byte[] audio)
        {
            var id = Bass.SampleLoad(audio, 0, audio.Length, 128, BassFlags.Default);
            var se = new BassSoundEffect(id, audio, this);

            OnUnload += se.Dispose;

            return se;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            OnVolumeChanged?.Invoke();
        }

        public void SetSoundEffectVolume(float volume)
        {
            effectVolume = volume;
            OnVolumeChanged?.Invoke();
        }

        public void SetSoundTrackVolume(float volume)
        {
            trackVolume = volume;
            OnVolumeChanged?.Invoke();
        }

        public void Unload()
        {
            OnUnload?.Invoke();
        }

        public void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}
