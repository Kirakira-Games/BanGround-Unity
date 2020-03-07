using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace AudioProvider
{
    public class BassSoundTrack : ISoundTrack
    {
        int _internalChannelID;
        GCHandle pinnedObject;
        BassAudioProvider _provider;

        bool _isLooping, noFade;
        long _loopstart, _loopend, _loopendms;

        float _volume = 1.0f;

        internal BassSoundTrack(int id, GCHandle audio, BassAudioProvider provider)
        {
            _internalChannelID = id;
            pinnedObject = audio;
            _provider = provider;
        }

        public uint GetLength()
        {
            var b = Bass.BASS_ChannelGetLength(_internalChannelID, BASSMode.BASS_POS_BYTES);
            var s = Bass.BASS_ChannelBytes2Seconds(_internalChannelID, b);
            var ms = s * 1000;

            return (uint)ms;
        }

        public uint GetPlaybackTime()
        {
            var b = Bass.BASS_ChannelGetPosition(_internalChannelID, BASSMode.BASS_POS_BYTES);
            var s = Bass.BASS_ChannelBytes2Seconds(_internalChannelID, b);
            var ms = s * 1000;

            return (uint)ms;
        }

        public PlaybackStatus GetStatus()
        {
            var status = Bass.BASS_ChannelIsActive(_internalChannelID);

            switch (status)
            {
                case BASSActive.BASS_ACTIVE_PAUSED:
                    return PlaybackStatus.Paused;
                case BASSActive.BASS_ACTIVE_PLAYING:
                    return PlaybackStatus.Playing;
                default:
                    return PlaybackStatus.Stopped;
            }
        }

        public void Pause()
        {
            Bass.BASS_ChannelPause(_internalChannelID);
        }

        public void Play()
        {
            Bass.BASS_ChannelPlay(_internalChannelID, false);
        }

        public void Restart()
        {
            if (_isLooping)
                Bass.BASS_ChannelSetPosition(_internalChannelID, _loopstart);
            else
                Bass.BASS_ChannelSetPosition(_internalChannelID, 0);

            Play();
        }

        [Obsolete]
        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void SetLoopingPoint(uint start, uint end, bool noFade)
        {
            _loopstart = Bass.BASS_ChannelSeconds2Bytes(_internalChannelID, start / 1000.0);
            _loopend = Bass.BASS_ChannelSeconds2Bytes(_internalChannelID, end / 1000.0);
            _loopendms = end;
            _isLooping = true;
            this.noFade = noFade;

            Bass.BASS_ChannelSetPosition(_internalChannelID, _loopstart);
        }

        public void SetPlaybackTime(uint time)
        {
            var b = Bass.BASS_ChannelSeconds2Bytes(_internalChannelID, time / 1000.0);
            Bass.BASS_ChannelSetPosition(_internalChannelID, b);
        }

        public void SetTimeScale(float scale, bool noPitchShift)
        {
            var info = Bass.BASS_ChannelGetInfo(_internalChannelID);

            Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_TEMPO, 0);
            Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, info.freq);

            if (noPitchShift)
            {
                var variable = (scale - 1) * 100;
                Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_TEMPO, variable);
            }
            else
            {
                Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, info.freq * scale);
            }
        }

        public void SetVolume(float volume)
        {
            this._volume = volume;
            VolumeChanged();
        }

        public void Stop()
        {
            Bass.BASS_ChannelStop(_internalChannelID);
        }

        public void Dispose()
        {
            Stop();
            Bass.BASS_StreamFree(_internalChannelID);

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
                if (!noFade)
                {
                    float volume = Math.Max(diff, 0) / 2000.0f;
                    Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_VOL, volume * _volume * _provider.trackVolume * _provider.masterVolume);
                }


                if (diff < -750)
                {
                    if (!noFade)
                        VolumeChanged();

                    Bass.BASS_ChannelSetPosition(_internalChannelID, _loopstart);
                }
            }
        }

        internal void VolumeChanged()
        {
            Bass.BASS_ChannelSetAttribute(_internalChannelID, BASSAttribute.BASS_ATTRIB_VOL, _volume * _provider.trackVolume * _provider.masterVolume);
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
            Bass.BASS_SampleFree(_internalSound);

            _provider.OnUnload -= Dispose;
        }

        public void PlayOneShot()
        {
            var channel = Bass.BASS_SampleGetChannel(_internalSound, false);
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, _provider.effectVolume * _provider.masterVolume);
            Bass.BASS_ChannelPlay(channel, false);
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
            Bass.BASS_Init(-1, sampleRate, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            if (bufferLength != 0) 
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, (int)bufferLength);
        }

        public ISoundTrack StreamTrack(byte[] audio)
        {
            var pinnedObject = GCHandle.Alloc(audio, GCHandleType.Pinned);
            var pinnedObjectPtr = pinnedObject.AddrOfPinnedObject();

            var id = Bass.BASS_StreamCreateFile(pinnedObjectPtr, 0, audio.Length, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
            var fxid = BassFx.BASS_FX_TempoCreate(id, BASSFlag.BASS_FX_FREESOURCE);

            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_USE_QUICKALGO, 1);
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_OVERLAP_MS, 4);
            Bass.BASS_ChannelSetAttribute(id, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_SEQUENCE_MS, 30);

            var st = new BassSoundTrack(fxid, pinnedObject, this);

            OnUpdate += st.Update;
            OnVolumeChanged += st.VolumeChanged;
            OnUnload += st.Dispose;

            st.VolumeChanged();

            return st;
        }

        public ISoundEffect PrecacheSE(byte[] audio)
        {
            var id = Bass.BASS_SampleLoad(audio, 0, audio.Length, 128, BASSFlag.BASS_DEFAULT);
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
