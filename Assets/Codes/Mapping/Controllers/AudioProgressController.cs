using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AudioProvider;
using System;
using FMOD;

namespace BGEditor
{
    public class AudioProgressController : CoreMonoBehaviour
    {
        public Text TimeText;
        public Slider AudioProgressSlider;
        public Slider PlaybackRateSlider;
        public Text PlayButtonText;

        public bool canSeek => bgm != null && bgm.GetStatus() != PlaybackStatus.Unknown && bgm.GetStatus() != PlaybackStatus.Playing;

        public uint audioLength { get; private set; }

        private ISoundTrack bgm => AudioManager.Instance.gameBGM;
        private ISoundEffect singleSE;
        private ISoundEffect flickSE;
        private float lastBeat;
        private static KVarRef cl_sestyle = new KVarRef("cl_sestyle");

        public static float TimeToScrollPos(float time)
        {
            float beat = Timing.TimeToBeat(time);
            return Editor.barHeight * beat;
        }

        public static float ScrollPosToTime(float pos)
        {
            return Timing.BeatToTime(pos / Editor.barHeight);
        }

        public void Refresh()
        {
            audioLength = bgm.GetLength();
        }

        public void UpdateDisplay()
        {
            int time = Mathf.RoundToInt(ScrollPosToTime(Editor.scrollPos) * 1000);
            time = Mathf.Clamp(time, 0, (int)audioLength - 1);
            int ms = time % 1000;
            int hour = time / 1000;
            int s = hour % 60;
            hour /= 60;
            int m = hour % 60;
            hour /= 60;
            TimeText.text = $"{hour:D1}:{m:D2}:{s:D2}.{ms:D3}";
            if (canSeek && Math.Abs(time - bgm.GetPlaybackTime()) > 3)
                bgm.SetPlaybackTime((uint)time);
            AudioProgressSlider.SetValueWithoutNotify((float)time / audioLength);
        }

        public void Play()
        {
            lastBeat = float.NaN;
            bgm.Play();
            PlayButtonText.text = "Pause";
        }

        public void Pause()
        {
            bgm.Pause();
            PlayButtonText.text = "Play";
        }

        public void Switch()
        {
            if (bgm == null || bgm.GetStatus() == PlaybackStatus.Unknown)
                return;
            if (bgm.GetStatus() == PlaybackStatus.Playing)
                Pause();
            else
                Play();
        }

        public void ChangePlaybackRate(float value)
        {
            value = Mathf.InverseLerp(PlaybackRateSlider.minValue, PlaybackRateSlider.maxValue, value);
            value = Mathf.Lerp(0.25f, 1f, value);
            bgm.SetTimeScale(value, true);
        }

        public async void Init(byte[] audio)
        {
            var hack = SettingAndMod.instance;
            // Load BGM
            AudioManager.Instance.gameBGM = await AudioManager.Provider.StreamTrack(audio);
            bgm.Play();
            bgm.Pause();

            // Add listeners
            AudioProgressSlider.onValueChanged.AddListener((value) => Core.SeekGrid(TimeToScrollPos(value * audioLength / 1000f)));
            PlaybackRateSlider.onValueChanged.AddListener(ChangePlaybackRate);

            Core.onGridMoved.AddListener(UpdateDisplay);
            Core.onTimingModified.AddListener(UpdateDisplay);
            Core.onAudioLoaded.Invoke();

            Refresh();
        }

        private async void Start()
        {
            // Load SE
            singleSE = await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) + "/perfect.wav").bytes);
            flickSE = await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) + "/flick.wav").bytes);

            lastBeat = float.NaN;
        }

        private void Update()
        {
            if (bgm == null)
                return;
            if (bgm.GetStatus() == PlaybackStatus.Playing)
            {
                float time = bgm.GetPlaybackTime() / 1000f;
                float beat = Timing.TimeToBeat(time);
                Core.SeekGrid(beat * Editor.barHeight, true);
                if (!float.IsNaN(lastBeat))
                {
                    Chart.groups.ForEach(group => group.notes.ForEach(note =>
                    {
                        if (note.type == NoteType.BPM)
                            return;
                        float cur = ChartUtility.BeatToFloat(note.beat);
                        if (cur < lastBeat || cur >= beat)
                            return;
                        if (note.type == NoteType.Flick)
                            flickSE.PlayOneShot();
                        else
                            singleSE.PlayOneShot();
                    }));
                }
                lastBeat = beat;
            }
            else if (PlayButtonText.text == "Pause")
            {
                lastBeat = float.NaN;
                Pause();
            }
        }
    }
}