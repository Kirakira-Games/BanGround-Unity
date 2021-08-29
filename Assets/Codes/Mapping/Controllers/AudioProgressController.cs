using UnityEngine;
using UnityEngine.UI;
using AudioProvider;
using System;
using Zenject;
using System.IO;
using BanGround;
using NoteType = V2.NoteType;

namespace BGEditor
{
    public class AudioProgressController : MonoBehaviour, IAudioProgressController
    {
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IAudioManager audioManager;
        [Inject]
        private IChartLoader chartLoader;
        [Inject]
        private IResourceLoader resourceLoader;
        [Inject]
        private IChartCore Core;
        [Inject]
        private IEditorInfo Editor;

        public Text TimeText;
        public Slider AudioProgressSlider;
        public Slider PlaybackRateSlider;
        public Button PlayButton;
        public Sprite PlayImg;
        public Sprite PauseImg;

        public bool canSeek => bgm != null && bgm.GetStatus() != PlaybackStatus.Unknown && bgm.GetStatus() != PlaybackStatus.Playing;

        public uint audioLength { get; private set; }

        private ISoundTrack bgm => audioManager.gameBGM;
        private ISoundEffect singleSE;
        private ISoundEffect flickSE;
        private float lastBeat;
        private float lastTime;
        private float lastTimeRecorded;

        public float TimeToScrollPos(float time)
        {
            float beat = Core.chart.TimeToBeat(time);
            return Editor.barHeight * beat;
        }

        public float ScrollPosToTime(float pos)
        {
            return Core.chart.BeatToTime(pos / Editor.barHeight);
        }

        public void Refresh()
        {
            audioLength = bgm.GetLength();
            float expectedLength = audioLength / 1000f;
            var mHeader = dataLoader.GetMusicHeader(chartLoader.header.mid);
            if (!NoteUtility.Approximately(mHeader.length, expectedLength))
            {
                mHeader.length = expectedLength;
                dataLoader.SaveHeader(mHeader);
            }
        }

        public void UpdateDisplay(bool isUser)
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
            if (isUser || (canSeek && Math.Abs(time - bgm.GetPlaybackTime()) > 3))
                bgm.SetPlaybackTime((uint)time);
            AudioProgressSlider.SetValueWithoutNotify((float)time / audioLength);
        }

        public void Play()
        {
            lastBeat = float.NaN;
            bgm.Play();

            PlayButton.image.sprite = PauseImg;
        }

        public void Pause()
        {
            bgm.Pause();
            PlayButton.image.sprite = PlayImg;
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

        public void IncreasePlaybackRate(int delta)
        {
            PlaybackRateSlider.value = Mathf.Clamp(PlaybackRateSlider.value + delta, PlaybackRateSlider.minValue, PlaybackRateSlider.maxValue);
        }

        public void IncreasePosition(float delta)
        {
            float value = AudioProgressSlider.value + delta * 1000 / audioLength;
            value = Mathf.Clamp01(value);
            AudioProgressSlider.value = value;
            Core.onUserChangeAudioProgress.Invoke();
        }

        [Inject]
        private IFileSystem fs;

        public async void Init()
        {
            byte[] audio =  fs.GetFile(dataLoader.GetMusicPath(chartLoader.header.mid)).ReadToEnd();
            // Load BGM
            audioManager.gameBGM = await audioManager.Provider.StreamTrack(audio);
            bgm.Play();
            bgm.Pause();

            // Add listeners
            AudioProgressSlider.onValueChanged.AddListener((value) =>
            {
                Core.SeekGrid(TimeToScrollPos(value * audioLength / 1000f), true);
                Core.onUserChangeAudioProgress.Invoke();
            });
            PlaybackRateSlider.onValueChanged.AddListener(ChangePlaybackRate);

            Core.onGridMoved.AddListener(() => UpdateDisplay(false));
            Core.onTimingModified.AddListener(() => UpdateDisplay(false));
            Core.onUserChangeAudioProgress.AddListener(() => UpdateDisplay(true));
            Core.onAudioLoaded.Invoke();

            Refresh();
        }

        private async void Start()
        {
            // Load SE
            singleSE = await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("perfect.wav").bytes);
            flickSE = await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("flick.wav").bytes);

            lastBeat = float.NaN;
            lastTime = float.NaN;
        }

        private void Update()
        {
            if (bgm == null)
                return;
            if (bgm.GetStatus() == PlaybackStatus.Playing)
            {
                float time = bgm.GetPlaybackTime() / 1000f;
                if (NoteUtility.Approximately(time, lastTime))
                {
                    time += Time.realtimeSinceStartup - lastTimeRecorded;
                }
                else
                {
                    lastTime = time;
                    lastTimeRecorded = Time.realtimeSinceStartup;
                }
                float beat = Core.chart.TimeToBeat(time);
                Core.SeekGrid(beat * Editor.barHeight, true);
                if (Editor.isSEOn && !float.IsNaN(lastBeat))
                {
                    Core.group.notes.ForEach(note =>
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
                    });
                }
                lastBeat = beat;
            }
            else if (ReferenceEquals(PlayButton.image.sprite, PauseImg))
            {
                lastBeat = float.NaN;
                lastTime = float.NaN;
                lastTimeRecorded = float.NaN;
                Pause();
            }
        }

        private void OnDestroy()
        {
            singleSE?.Dispose();
            singleSE = null;
            flickSE?.Dispose();
            flickSE = null;
            audioManager.gameBGM?.Dispose();
            audioManager.gameBGM = null;
        }
    }
}
