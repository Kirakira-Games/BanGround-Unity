using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using UniRx.Async;
using Zenject;

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行

namespace AudioProvider
{
    internal class FMODUtil
    {
        public static IMessageBannerController messageBannerController;

        internal static void ErrCheck(RESULT result)
        {
            if (result != RESULT.OK)
                //throw new Exception($"FMOD Error! Result code: {Enum.GetName(typeof(RESULT), result)}");
                messageBannerController.ShowMsg(LogLevel.ERROR, Enum.GetName(typeof(RESULT), result), false);
        }
    }

    public class FmodSoundTrack : ISoundTrack
    {
        internal static ChannelGroup stGroup;
        internal byte[] bytes;

        Sound _internalSound;
        Channel _internalChannel;
        DSP _internalDSP;
        FMOD.System _internalSystem;
        FmodAudioProvider parent;

        bool isLooping, noFade;

        float volume = 1.0f;

        internal FmodSoundTrack(Sound sound, FMOD.System system, FmodAudioProvider provider)
        {
            _internalSound = sound;
            _internalSystem = system;
            parent = provider;

            FMODUtil.ErrCheck(
                system.playSound(sound, stGroup, true, out _internalChannel)
            );

            FMODUtil.ErrCheck(
                _internalSound.setLoopCount(0)
            );
        }

        public void Dispose()
        {
            _internalSound.release();
            _internalSound.release();

            parent.OnUnload -= Unload;
            parent.OnUpdate -= Update;
            parent.OnVolumeChanged -= VolumeChanged;
        }

        public uint GetLength()
        {
            FMODUtil.ErrCheck(
                _internalSound.getLength(out uint length, TIMEUNIT.MS)
            );

            return length;
        }

        public uint GetPlaybackTime()
        {
            FMODUtil.ErrCheck(
                _internalChannel.getPosition(out uint position, TIMEUNIT.MS)
            );

            return position;
        }

        public PlaybackStatus GetStatus()
        {
            FMODUtil.ErrCheck(
                _internalChannel.isPlaying(out bool playing)
            );

            if (!playing)
                return PlaybackStatus.Stopped;

            FMODUtil.ErrCheck(
                _internalChannel.getPaused(out bool paused)
            );

            if (paused)
                return PlaybackStatus.Paused;

            return PlaybackStatus.Playing;
        }

        public void Pause()
        {
            FMODUtil.ErrCheck(
                _internalChannel.setPaused(true)
            );
        }

        [Obsolete]
        public void Resume()
        {
            FMODUtil.ErrCheck(
                _internalChannel.setPaused(false)
            );
        }

        public void Play()
        {
            FMODUtil.ErrCheck(
                _internalChannel.setPaused(false)
            );
        }

        public void Restart()
        {
            if (isLooping)
            {
                FMODUtil.ErrCheck(
                    _internalChannel.setPosition(loopingStart, TIMEUNIT.MS)
                    );
            }
            else
            {
                FMODUtil.ErrCheck(
                    _internalChannel.setPosition(0, TIMEUNIT.MS)
                );
            }

            FMODUtil.ErrCheck(
                _internalChannel.setPaused(false)
            );
        }

        uint loopingEnd, loopingStart;

        public void SetLoopingPoint(uint start, uint end, bool noFade)
        {
            loopingEnd = end;
            loopingStart = start;
            isLooping = true;
            this.noFade = noFade;
            if (noFade)
            {
                FMODUtil.ErrCheck(
                    _internalChannel.setLoopCount(-1)
                );

                FMODUtil.ErrCheck(
                    _internalChannel.setLoopPoints(start, TIMEUNIT.MS, end, TIMEUNIT.MS)
                );
            }
            FMODUtil.ErrCheck(
                    _internalChannel.setPosition(loopingStart, TIMEUNIT.MS)
                    );
        }

        public void SetPlaybackTime(uint time)
        {
            FMODUtil.ErrCheck(
                _internalChannel.setPosition(time, TIMEUNIT.MS)
            );
            lastTime = time;
        }

        public void SetTimeScale(float scale, bool noPitchShift)
        {
            FMODUtil.ErrCheck(
                _internalChannel.setPitch(scale)
            );

            if (noPitchShift)
            {
                if (_internalDSP.handle == IntPtr.Zero)
                {
                    FMODUtil.ErrCheck(
                        _internalSystem.createDSPByType(DSP_TYPE.PITCHSHIFT, out _internalDSP)
                    );
                    FMODUtil.ErrCheck(
                        _internalChannel.addDSP(0, _internalDSP)
                    );
                }

                FMODUtil.ErrCheck(
                    _internalDSP.setParameterFloat((int)DSP_PITCHSHIFT.PITCH, 1.0f / scale)
                );
            }

        }

        public void Stop()
        {
            FMODUtil.ErrCheck(
                _internalChannel.stop()
            );
        }

        ulong fadeStartClock = 0, fadeEndClock = 0;
        uint lastTime = 0;
        internal void Update()
        {
            var status = GetStatus();

            if(!isLooping && status == PlaybackStatus.Playing)
            {
                uint time = GetPlaybackTime();
                if (time < lastTime) Pause();
                else lastTime = time;
            }

            if (!isLooping || noFade || status != PlaybackStatus.Playing)
                return;

            if (fadeStartClock == 0 && GetPlaybackTime() > loopingEnd - 1500)
            {
                FMODUtil.ErrCheck(_internalSystem.getSoftwareFormat(out int rate, out _, out _));

                FMODUtil.ErrCheck(_internalChannel.getDSPClock(out _, out ulong dspclock));
                FMODUtil.ErrCheck(_internalChannel.addFadePoint(dspclock, 1.0f));
                FMODUtil.ErrCheck(_internalChannel.addFadePoint(dspclock + (ulong)(rate * 1.4f), 0.0f));

                fadeStartClock = dspclock;
                fadeEndClock = dspclock + (ulong)(rate * 2.0f);
            }

            if (fadeStartClock != 0 && GetPlaybackTime() > loopingEnd)
            {
                _internalChannel.removeFadePoints(fadeStartClock - 1, fadeEndClock + 1);

                SetPlaybackTime(loopingStart);

                FMODUtil.ErrCheck(_internalSystem.getSoftwareFormat(out int rate, out _, out _));

                FMODUtil.ErrCheck(_internalChannel.getDSPClock(out _, out ulong dspclock));
                FMODUtil.ErrCheck(_internalChannel.addFadePoint(dspclock + (ulong)(rate * 0.5f), 1.0f));

                fadeStartClock = 0;
            }
        }

        internal void VolumeChanged()
        {
            _internalChannel.setVolume(parent.masterVolume * parent.trackVolume * volume);
        }

        internal void Unload()
        {
            Dispose();
        }

        public void SetVolume(float volume)
        {
            this.volume = volume;
            VolumeChanged();
        }
    }

    class FmodSoundEffect : ISoundEffect
    {
        internal static ChannelGroup seGroup;
        internal byte[] bytes;

        Sound _internalSound;
        FMOD.System _internalSystem;
        FmodAudioProvider parent;
        float volume;
        SEType _type;

        internal FmodSoundEffect(Sound sound, FMOD.System system, FmodAudioProvider provider, SEType type)
        {
            _internalSound = sound;
            _internalSystem = system;
            _type = type;
            parent = provider;
        }

        public void Dispose()
        {
            _internalSound.release();
        }

        public void PlayOneShot()
        {
            _internalSystem.playSound(_internalSound, seGroup, true, out Channel channel);
            channel.setVolume(volume);
            channel.setPaused(false);
        }

        internal void VolumeChanged()
        {
            volume = parent.masterVolume * parent.effectVolume[(int)_type];
        }

        internal void Unload()
        {
            Dispose();
        }
    }

    class FmodAudioProvider : IAudioProvider
    {
        FMOD.System fmodSystem;

        internal float trackVolume = 1.0f;
        internal float[] effectVolume = { 1.0f, 1.0f };
        internal float masterVolume = 1.0f;

        public delegate void FmodEventHandler();
        public event FmodEventHandler OnUpdate;
        public event FmodEventHandler OnVolumeChanged;
        public event FmodEventHandler OnUnload;

        [Inject(Id = "snd_output")]
        private KVar snd_output;
        [Inject]
        private IMessageBannerController messageBannerController;

        public void Init(int sampleRate, uint bufferLength)
        {
            FMODUtil.messageBannerController = messageBannerController;

            int ao_output = snd_output;
            FMODUtil.ErrCheck(
                Factory.System_Create(out fmodSystem)
            );

            if (bufferLength != 0)
            {
                fmodSystem.setDSPBufferSize(bufferLength, 4);
            }
            else
            {
                fmodSystem.setDSPBufferSize(256, 4);
            }
            fmodSystem.setSoftwareFormat(sampleRate, SPEAKERMODE.STEREO, 0);
            fmodSystem.setOutput((OUTPUTTYPE)ao_output);
            UnityEngine.Debug.Log($"Init FMOD with OUTPUTTYPE:{Enum.GetName(typeof(OUTPUTTYPE), ao_output)}");

            FMODUtil.ErrCheck(
                fmodSystem.init(512, INITFLAGS.NORMAL, IntPtr.Zero)
            );

            fmodSystem.createChannelGroup("Tracks", out FmodSoundTrack.stGroup);
            fmodSystem.createChannelGroup("SoundEffects", out FmodSoundEffect.seGroup);
        }
        public async UniTask<ISoundEffect> PrecacheSE(byte[] audio, SEType type)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)audio.Length;

            FMODUtil.ErrCheck(
                fmodSystem.createSound(audio, MODE.OPENMEMORY | MODE.CREATESAMPLE | MODE.LOOP_OFF, ref exinfo, out Sound sound)
            );

            var result = new FmodSoundEffect(sound, fmodSystem, this, type);
            result.bytes = audio;

            OnVolumeChanged += result.VolumeChanged;
            result.VolumeChanged();

            OnUnload += result.Unload;

            return result;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            OnVolumeChanged();
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

        public async UniTask<ISoundTrack> StreamTrack(byte[] audio)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)audio.Length;

            FMODUtil.ErrCheck(
                fmodSystem.createStream(audio, MODE.ACCURATETIME | MODE.LOOP_NORMAL | MODE.OPENMEMORY, ref exinfo, out Sound sound)
            );
            var result = new FmodSoundTrack(sound, fmodSystem, this);

            result.bytes = audio;
            OnUpdate += result.Update;
            OnVolumeChanged += result.VolumeChanged;
            OnUnload += result.Unload;

            result.VolumeChanged();

            return result;
        }

        public void Unload()
        {
            OnUnload?.Invoke();
            FMODUtil.ErrCheck(fmodSystem.release());
        }

        public void Update()
        {
            FMODUtil.ErrCheck(fmodSystem.update());
            OnUpdate?.Invoke();
        }
    }
}
