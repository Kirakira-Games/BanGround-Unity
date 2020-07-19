using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NVorbis;
using System.IO;
using System;
using UnityEngine.Timeline;
using System.Linq;
using WebSocketSharp;

namespace AudioProvider
{
    public class UnityAudioClip : ISoundEffect, ISoundTrack
    {
        static ulong totalClips = 0UL;
        AudioClip clip;
        AudioSource source;
        SEType type;
        PureUnityAudioProvider provider;

        bool isDisposed = false;

        public UnityAudioClip(float[] data, int sampleRate, int channels, GameObject obj, SEType type, PureUnityAudioProvider provider)
        {
            totalClips++;

            this.type = type;
            this.provider = provider;

            clip = AudioClip.Create($"UAP_clip{totalClips}", data.Length / channels, channels, sampleRate, false);
            clip.SetData(data, 0);
            clip.LoadAudioData();

            source = obj.AddComponent<AudioSource>();
            source.clip = clip;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            GameObject.Destroy(source.gameObject);
        }

        public uint GetLength()
        {
            if (isDisposed)
                return 0;

            return (uint)(clip.length * 1000f);
        }

        public uint GetPlaybackTime()
        {
            if (isDisposed)
                return 0;

            return (uint)(source.time * 1000f);
        }
        public void Pause()
        {
            if (isDisposed)
                return;

            source.Pause();
        }
        public void Play()
        {
            if (isDisposed)
                return;

            source.Play();
        }
        public void PlayOneShot()
        {
            if (isDisposed)
                return;

            if (provider.effectVolume[(int)type] == 0)
                return;

            source.PlayOneShot(clip);
        }
        public void Restart()
        {
            if (isDisposed)
                return;

            source.Play();
        }
        public void Resume()
        {
            if (isDisposed)
                return;

            source.UnPause();
        }
        public void SetPlaybackTime(uint time)
        {
            if (isDisposed)
                return;

            source.time = time / 1000.0f;
        }

        public void SetVolume(float volume)
        {
            if (isDisposed)
                return;

            source.volume = volume;
        }

        public void Stop()
        {
            if (isDisposed)
                return;

            source.Stop();
        }

        public PlaybackStatus GetStatus()
        {
            if (isDisposed)
                return PlaybackStatus.Unknown;

            if (source.isPlaying)
                return PlaybackStatus.Playing;

            if (source.time == 0)
                return PlaybackStatus.Stopped;

            return PlaybackStatus.Paused;
        }

        public void SetLoopingPoint(uint start, uint end, bool noFade)
        {
            if (isDisposed)
                return;

            // TODO: uint start, uint end, bool noFade, need to do this like bass
            source.loop = true;
        }

        public void SetTimeScale(float scale, bool noPitchShift)
        {
            if (isDisposed)
                return;

            // TODO: bool noPitchShift, maybe able to do this via modifiying data in audioclip, requires a math master like @KCFindstr
            source.pitch = scale;
        }

        internal void VolumeChanged()
        {
            if (isDisposed)
                return;

            if (type == SEType.Unknown)
                source.volume = provider.masterVolume * provider.trackVolume;
            else
                source.volume = provider.masterVolume * provider.effectVolume[(int)type];
        }

        internal void Unload()
        {
            Dispose();
        }
    }

    public class PureUnityAudioProvider : IAudioProvider
    {
        internal float trackVolume = 1.0f;
        internal float[] effectVolume = { 1.0f, 1.0f };
        internal float masterVolume = 1.0f;
        private GameObject audioMgr;

        public delegate void UAPEventHandler();
        public event UAPEventHandler OnVolumeChanged;
        public event UAPEventHandler OnUnload;

        public void Init(int sampleRate, uint bufferLength)
        {
            var cfg = AudioSettings.GetConfiguration();
            cfg.dspBufferSize = (int)bufferLength;
            cfg.sampleRate = sampleRate;
            cfg.numRealVoices = 32;
            cfg.numVirtualVoices = 1024;
            cfg.speakerMode = AudioSpeakerMode.Stereo;

            AudioSettings.Reset(cfg);

            audioMgr = GameObject.Find("AudioMgr");
            audioMgr.AddComponent<AudioListener>();
        }

        private UnityAudioClip LoadOggClip(byte[] audio, SEType type)
        {
            List<float> samples = new List<float>();
            int channels = 0;
            int sampleRate = 0;
            long sampleCount = 0;

            using (var decoder = new VorbisReader(new MemoryStream(audio), true))
            {
                channels = decoder.Channels;
                sampleRate = decoder.SampleRate;
                sampleCount = decoder.TotalSamples;

                var tmp = new float[channels * sampleRate / 5];

                // go grab samples
                int count;
                while ((count = decoder.ReadSamples(tmp, 0, tmp.Length)) > 0)
                    samples.AddRange(tmp.SubArray(0, count));
            }

            var gobj = new GameObject("UAPAudio");
            GameObject.DontDestroyOnLoad(gobj);

            gobj.transform.parent = audioMgr.transform;

            return new UnityAudioClip(samples.ToArray(), sampleRate, channels, gobj, type, this);
        }

        public ISoundEffect PrecacheSE(byte[] audio, SEType type)
        {
            var result = LoadOggClip(audio, type);

            OnVolumeChanged += result.VolumeChanged;
            result.VolumeChanged();

            OnUnload += result.Unload;

            return result;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            OnVolumeChanged?.Invoke();
        }

        public void SetSoundEffectVolume(float volume, SEType type)
        {
            effectVolume[(int)type] = volume;
            OnVolumeChanged?.Invoke();
        }

        public void SetSoundTrackVolume(float volume)
        {
            trackVolume = volume;
            OnVolumeChanged?.Invoke();
        }

        public ISoundTrack StreamTrack(byte[] audio)
        {
            var result = LoadOggClip(audio, SEType.Unknown);

            OnVolumeChanged += result.VolumeChanged;
            result.VolumeChanged();

            OnUnload += result.Unload;

            return result;
        }

        public void Unload()
        {
            OnUnload?.Invoke();   
        }

        public void Update()
        {
            
        }
    }
}