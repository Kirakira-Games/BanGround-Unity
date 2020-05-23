using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AudioProvider;
using System;
using System.Threading;

namespace BGEditor
{
    public class AudioProgressController : CoreMonoBehaviour
    {
        public Text TimeText;
        public Slider AudioProgressSlider;
        public Slider PlaybackRateSlider;
        public Text PlayButtonText;

        public bool canSeek => bgm != null && bgm.GetStatus() != PlaybackStatus.Unknown && bgm.GetStatus() != PlaybackStatus.Playing;

        private uint audioLength;

        private ISoundTrack bgm => AudioManager.Instance.gameBGM;

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
            int ms = time % 1000;
            time /= 1000;
            int s = time % 60;
            time /= 60;
            int m = time % 60;
            time /= 60;
            TimeText.text = $"{time:D1}:{m:D2}:{s:D2}.{ms:D3}";
            if (canSeek && Math.Abs(time - bgm.GetPlaybackTime()) > 3)
                bgm.SetPlaybackTime((uint)time);
            AudioProgressSlider.SetValueWithoutNotify(time / 1000f / audioLength);
        }

        public void Play()
        {
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

        private void Awake()
        {
            byte[] audio = Resources.Load<TextAsset>("等mapping测完就把它杀了.ogg").bytes;
            AudioManager.Instance.gameBGM = AudioManager.Provider.StreamTrack(audio);
            bgm.Play();
            bgm.Pause();
            AudioProgressSlider.onValueChanged.AddListener((value) => Core.SeekGrid(TimeToScrollPos(value * audioLength)));
            PlaybackRateSlider.onValueChanged.AddListener(ChangePlaybackRate);

            Core.onGridMoved.AddListener(UpdateDisplay);
            Core.onTimingModified.AddListener(UpdateDisplay);

            Refresh();
        }

        private void Update()
        {
            if (bgm.GetStatus() == PlaybackStatus.Playing)
            {
                Core.SeekGrid(TimeToScrollPos(bgm.GetPlaybackTime() / 1000f), true);
            }
        }
    }
}