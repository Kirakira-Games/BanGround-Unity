using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using V2;
using Zenject;

namespace BGEditor
{
    public class GridController : MonoBehaviour, IPointerClickHandler, IGridController
    {
        [Inject]
        private IChartCore Core;
        [Inject]
        private IObjectPool Pool;
        [Inject]
        private IEditorInfo Editor;
        [Inject]
        private IEditNoteController Notes;
        [Inject]
        private IMessageBannerController messageBannerController;

        public float VPadding;
        public float LineBoundingHeight;
        public int BPMFontSize;
        public Font fontFamily;

        public int StartBar { get; private set; }
        public int EndBar { get; private set; }

        public GameObject GridParent;
        public TimingPointEdit TimingPointEdit;

        public Color Div1Color;
        public Color Div2Color;
        public Color Div3Color;
        public Color Div4Color;
        public Color OtherColor;

        private LinkedList<GameObject> objectsInUse;
        private float gridHeight;
        private float gridWidth;
        private float laneWidth;
        private RectTransform parentRect;

        private void Awake()
        {
            Core.onChartLoaded.AddListener(Refresh);
        }

        private void Start()
        {
            objectsInUse = new LinkedList<GameObject>();
            parentRect = GetComponent<RectTransform>();

            Core.onChartLoaded.AddListener(Refresh);
            Core.onGridModifed.AddListener(Refresh);
            Core.onGridMoved.AddListener(Refresh);
            Core.onTimingPointModified.AddListener(Refresh);
            Core.onTimingModified.AddListener(Refresh);
            Core.onTimingGroupSwitched.AddListener(Refresh);
            var rect = GetComponent<RectTransform>().rect;
            gridHeight = rect.height;
            gridWidth = rect.width;
            laneWidth = gridWidth / 7;

            var obj = new GameObject("GridParent");
            obj.transform.SetParent(transform, false);
        }

        #region Create
        private GameObject CreateLine(float width, Vector2 anchoredPosition, Color color)
        {
            var ret = Pool.Create<Image>();
            ret.transform.SetParent(GridParent.transform, false);
            var rect = ret.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(gridWidth, width);
            rect.anchoredPosition = anchoredPosition;
            var image = ret.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            objectsInUse.AddLast(ret);
            return ret;
        }

        private Text CreateText(Vector2 anchoredPosition, string text)
        {
            var ret = Pool.Create<Text>();
            ret.transform.SetParent(transform, false);
            var rect = ret.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = anchoredPosition;
            rect.pivot = Vector2.right;
            rect.sizeDelta = new Vector2(120, 100);
            var txt = rect.GetComponent<Text>();
            txt.raycastTarget = false;
            txt.text = text;
            txt.font = fontFamily;
            txt.alignment = TextAnchor.MiddleRight;
            txt.fontSize = 48;
            objectsInUse.AddLast(ret);
            return txt;
        }

        private GridInfoText CreateGridInfo(Vector2 anchoredPosition, string text, bool isleft)
        {
            var ret = Pool.Create<GridInfoText>();
            var info = ret.GetComponent<GridInfoText>();
            if (isleft)
                info.ResetLeft(anchoredPosition, text);
            else
                info.ResetRight(anchoredPosition, text);
            var color = info.img.color;
            color.a = Editor.isSpeedView ? 1f : 0.5f;
            info.img.color = color;
            objectsInUse.AddLast(ret);
            return info;
        }
        #endregion

        #region Refresh
        private bool GetLaneBeat(Vector2 pos, out int lane, out int[] beat)
        {
            lane = -1;
            beat = new int[3];
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, pos, null, out var point);

            // Compute beat
            float actualY = point.y + Editor.scrollPos - VPadding;
            float unitgrid = (float)Editor.barHeight / Editor.gridDivision;
            int nearestline = Mathf.RoundToInt(actualY / unitgrid);
            if (nearestline < 0)
                return false;
            float expectedpos = nearestline * unitgrid;
            if (Mathf.Abs(expectedpos - actualY) > LineBoundingHeight)
                return false;
            beat[1] = nearestline;
            beat[2] = Editor.gridDivision;
            ChartUtility.NormalizeBeat(beat);

            // Compute track
            lane = Mathf.Clamp(Mathf.FloorToInt(point.x / laneWidth), 0, NoteUtility.LANE_COUNT - 1);
            return true;
        }

        public Vector2 GetLocalPosition(float x, float beat)
        {
            x = (x + 0.5f) * laneWidth;
            float y = beat * Editor.barHeight - (Editor.scrollPos - VPadding);
            return new Vector2(x, y);
        }

        public Vector2 GetLocalPosition(float x, int[] beat)
        {
            float floatbeat = ChartUtility.BeatToFloat(beat);
            return GetLocalPosition(x, floatbeat);
        }

        public void Refresh()
        {
            foreach (var obj in objectsInUse)
            {
                Pool.Destroy(obj);
            }
            objectsInUse.Clear();

            // Create grid
            float start = Editor.scrollPos - VPadding;
            float end = start + gridHeight;
            StartBar = Mathf.Max(0, Mathf.FloorToInt(start / Editor.barHeight));
            EndBar = Mathf.Max(0, Mathf.FloorToInt(end / Editor.barHeight)) + 1;
            for (int i = StartBar; i <= EndBar; i++)
            {
                for (int j = 0; j < Editor.gridDivision; j++)
                {
                    float y = ((float)j / Editor.gridDivision + i) * Editor.barHeight - start;
                    float width;
                    Color color;
                    if (j == 0)
                    {
                        width = 3;
                        color = Div1Color;
                        CreateText(new Vector2(-30, y), i.ToString());
                    }
                    else if (j * 2 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Div2Color;
                    }
                    else if (j * 3 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Div3Color;
                    }
                    else if (j * 4 % Editor.gridDivision == 0)
                    {
                        width = 2;
                        color = Div4Color;
                    }
                    else
                    {
                        width = 2;
                        color = OtherColor;
                    }
                    var obj = CreateLine(width, new Vector2(0, y), color);
                }
            }
            // Create timing
            foreach (var bpm in Core.chart.bpm)
            {
                float beat = ChartUtility.BeatToFloat(bpm.beat);
                if (beat < StartBar || beat > EndBar)
                    continue;

                float y = beat * Editor.barHeight - start;
                var info = CreateGridInfo(new Vector2(0, y), $"BPM:{Mathf.RoundToInt(bpm.value * 1000) / 1000f}", true);
                info.rect.sizeDelta = new Vector2(160, 40);
            }
            // Create timing point info
            foreach (var point in Core.group.points)
            {
                float beat = ChartUtility.BeatToFloat(point.beat);
                beat = Mathf.Max(0, beat);
                if (beat < StartBar || beat > EndBar)
                    continue;

                float y = beat * Editor.barHeight - start;
                var info = CreateGridInfo(new Vector2(0, y), point.ToEditorString(), false);
                info.rect.sizeDelta = new Vector2(300, 40);
            }
            Notes.Refresh();
        }
        #endregion

        #region PointerClick
        private void OnPointerClickNoteView(PointerEventData eventData)
        {
            // Handle click on slide body
            var ray = new Ray(new Vector3(eventData.pressPosition.x, eventData.pressPosition.y, -1), Vector3.forward * 10);
            var hits = Physics2D.GetRayIntersectionAll(ray);
            foreach (var hit in hits)
            {
                var collider = hit.collider;
                if (collider.CompareTag("MappingSlideBody"))
                {
                    var body = collider.GetComponent<EditorSlideBody>();
                    if (body.enabled)
                    {
                        body.parent.OnClick(eventData);
                        return;
                    }
                }
            }
            // Other clicks
            if (GetLaneBeat(eventData.pressPosition, out var lane, out var beat))
            {
                if (eventData.button == PointerEventData.InputButton.Right || Editor.tool == EditorTool.Delete)
                {
                    Notes.UnselectAll();
                    return;
                }
                if (Editor.tool == EditorTool.Select)
                    return;
                var note = new V2.Note
                {
                    lane = Editor.yDivision == 0 ? lane : -1,
                    x = Editor.yDivision == 0 ? 0 : lane,
                    y = Editor.yPos,
                    beat = beat,
                    group = Editor.currentTimingGroup
                };
                if (Editor.tool == EditorTool.Single)
                {
                    note.type = NoteType.Single;
                }
                else if (Editor.tool == EditorTool.Flick)
                {
                    note.type = NoteType.Flick;
                }
                else if (Editor.tool == EditorTool.Slide)
                {
                    note.type = NoteType.Single;
                    note.tickStack = Notes.slideIdPool.RegisterNext();
                    var prev = Notes.singleSlideSelected;
                    if (prev != null)
                    {
                        var cmds = new CmdGroup();
                        cmds.Add(new CreateNoteCmd(note));
                        cmds.Add(new ConnectNoteCmd(Notes, prev.note, note));
                        if (Core.Commit(cmds))
                            return;
                        Notes.UnselectAll();
                        return;
                    }
                }
                if (Notes.selectedNotes.Count > 0)
                    Notes.UnselectAll();
                else
                    Core.Commit(new CreateNoteCmd(note));
                return;
            }
            else if (Editor.tool != EditorTool.Select || eventData.button == PointerEventData.InputButton.Right)
            {
                Notes.UnselectAll();
            }
        }

        public TimingPoint GetTimingPointByBeat(float beatf)
        {
            foreach (var point in Core.group.points)
            {
                point.beatf = ChartUtility.BeatToFloat(point.beat);
                if (NoteUtility.Approximately(beatf, Mathf.Max(0, point.beatf)))
                {
                    return point;
                }
            }
            return null;
        }

        private void OnPointerClickSpeedView(PointerEventData eventData)
        {
            // Click to change or add a timing point
            if (GetLaneBeat(eventData.pressPosition, out var _, out var beat))
            {
                float beatf = ChartUtility.BeatToFloat(beat);
                var Point = GetTimingPointByBeat(beatf);
                // Delete timing point
                if (eventData.button == PointerEventData.InputButton.Right || Editor.tool == EditorTool.Delete)
                {
                    if (Point != null)
                    {
                        if (Point.beat[0] >= 0)
                            Core.Commit(new RemoveTimingPointCmd(Point));
                        else
                            messageBannerController.ShowMsg(LogLevel.INFO, "Editor.CantRemoveFirstTimingPoint".L());
                    }
                    return;
                }
                // Edit existing timing point
                if (Point != null)
                {
                    TimingPointEdit.Point = Point;
                    TimingPointEdit.Show();
                    return;
                }
                // Add a timing point
                for (int i = 0; i < Core.group.points.Count; i++)
                {
                    var p = Core.group.points[i];
                    if (beatf < p.beatf)
                    {
                        Point = Core.group.points[i - 1].Copy();
                        break;
                    }
                    if (i == Core.group.points.Count - 1)
                    {
                        Point = Core.group.points[i].Copy();
                        break;
                    }
                }
                Point.beat = beat;
                Core.Commit(new AddTimingPointCmd(Point));
                return;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Editor.isSpeedView)
            {
                OnPointerClickSpeedView(eventData);
            }
            else
            {
                OnPointerClickNoteView(eventData);
            }
        }
        #endregion

        #region Handlers
        public void OnSwitchGridView(bool isOn)
        {
            Editor.isSpeedView = isOn;
            Core.onSpeedViewSwitched.Invoke();
            Refresh();
        }
        #endregion
    }
}
