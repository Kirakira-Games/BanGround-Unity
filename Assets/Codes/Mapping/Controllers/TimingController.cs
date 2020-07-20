using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

namespace BGEditor
{
    public class TimingController : CoreMonoBehaviour
    {
        public Button Blocker;
        public GameObject Template;
        public GameObject TimingWindow;
        public IntInput Offset;
        [HideInInspector]
        public List<V2.ValuePoint> BpmList = new List<V2.ValuePoint>();
        public List<V2.ValuePoint> currentBpmList => bpmLines.Select(line => new V2.ValuePoint
        {
            beat = line.Beat.beat,
            value = line.Bpm.value
        }).OrderBy(note => ChartUtility.BeatToFloat(note.beat)).ToList();

        private List<BPMLine> bpmLines = new List<BPMLine>();

        private ScrollRect scrollRect;
        private int waitScrollToBottom;

        private void Awake()
        {
            scrollRect = GetComponentInParent<ScrollRect>();
        }

        public void Show()
        {
            Blocker.gameObject.SetActive(true);
            TimingWindow.SetActive(true);
            BpmList.ForEach(point => AddLine(point.beat, point.value));
            Offset.SetValue(Chart.offset);
            RefreshIndex();
        }

        public void AddLine(int[] beat, float bpm)
        {
            var line = Instantiate(Template, transform).GetComponent<BPMLine>();
            line.Beat.SetValue(beat);
            line.Bpm.SetValue(bpm);
            line.Remove.onClick.AddListener(() => RemoveLine(line));
            bpmLines.Add(line);
            waitScrollToBottom = 2;
        }

        public void AddEmptyLine()
        {
            var last = bpmLines[bpmLines.Count - 1];
            AddLine(last.Beat.beat, last.Bpm.value);
            RefreshIndex();
        }

        public void RemoveLine(BPMLine line)
        {
            if (ReferenceEquals(line, bpmLines[0]))
                return;
            Destroy(line.gameObject);
            bpmLines.Remove(line);
            RefreshIndex();
        }

        public void Hide(bool save)
        {
            if (save)
            {
                BpmList = currentBpmList;
                Chart.offset = Offset.value;
                Core.onTimingModified.Invoke();
            }
            bpmLines.ForEach(line => Destroy(line.gameObject));
            bpmLines.Clear();
            TimingWindow.SetActive(false);
            Blocker.gameObject.SetActive(false);
        }

        public void RefreshIndex()
        {
            bpmLines[0].Remove.interactable = false;
            for (int i = 0; i < bpmLines.Count; i++)
                bpmLines[i].Index.text = "#" + (i + 1);
        }

        public float TimeToBeat(float time)
        {
            time -= Chart.offset / 1000f;
            if (time - NoteUtility.EPS <= 0)
                return 0;

            for (int i = 0; i < BpmList.Count; i++)
            {
                var bpm = BpmList[i];
                float timeperbeat = 60 / bpm.value;
                float start = ChartUtility.BeatToFloat(bpm.beat);
                float end = i == BpmList.Count - 1 ? 1e9f : ChartUtility.BeatToFloat(BpmList[i + 1].beat);
                float duration = (end - start) * timeperbeat;
                if (time <= duration)
                {
                    return start + time / timeperbeat;
                }
                time -= duration;
            }

            throw new ArgumentOutOfRangeException(time + " cannot be converted to beat.");
        }

        public float BeatToTime(float beat)
        {
            float ret = Chart.offset / 1000f;

            for (int i = 0; i < BpmList.Count; i++)
            {
                var bpm = BpmList[i];
                float start = ChartUtility.BeatToFloat(bpm.beat);
                float end = i == BpmList.Count - 1 ? 1e9f : ChartUtility.BeatToFloat(BpmList[i + 1].beat);
                float timeperbeat = 60 / bpm.value;
                if (beat <= end)
                {
                    ret += (beat - start) * timeperbeat;
                    return ret;
                }
                ret += (end - start) * timeperbeat;
            }

            throw new ArgumentOutOfRangeException(beat + " cannot be converted to time.");
        }

        public float BeatToTime(int[] beat) { return BeatToTime(ChartUtility.BeatToFloat(beat)); }

        public int BeatToTimeMS(float beat)
        {
            return Mathf.RoundToInt(BeatToTime(beat) * 1000);
        }

        public int BeatToTimeMS(int[] beat) { return BeatToTimeMS(ChartUtility.BeatToFloat(beat)); }

        private void Update()
        {
            if (waitScrollToBottom > 0)
            {
                waitScrollToBottom--;
                if (waitScrollToBottom == 0)
                    scrollRect.verticalNormalizedPosition = 0;
            }
        }
    }
}