using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Zenject;

namespace BGEditor
{
    public class TimingController : MonoBehaviour
    {
        public GameObject Template;
        public GameObject TimingWindow;
        public IntInput Offset;

        [Inject]
        private IChartCore Core;
        [Inject(Id = "Blocker")]
        private Button Blocker;

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
            if (TimingWindow.activeSelf)
                return;

            Blocker.gameObject.SetActive(true);
            TimingWindow.SetActive(true);
            BpmList.ForEach(point => AddLine(point.beat, point.value));
            Offset.SetValue(Core.chart.offset);
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
                var current = currentBpmList;
                BpmList.Clear();
                current.ForEach(BpmList.Add);
                Core.chart.offset = Offset.value;
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
