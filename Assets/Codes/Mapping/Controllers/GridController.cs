using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking.NetworkSystem;
using System;
using V2;

namespace BGEditor
{
    public class GridController : CoreMonoBehaviour, IPointerClickHandler
    {
        public float VPadding;
        public float LineBoundingHeight;
        public int BPMFontSize;

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

        private void Start()
        {
            objectsInUse = new LinkedList<GameObject>();
            parentRect = GetComponent<RectTransform>();

            Core.onGridModifed.AddListener(Refresh);
            Core.onGridMoved.AddListener(Refresh);
            Core.onTimingPointModified.AddListener(Refresh);
            Core.onTimingModified.AddListener(Refresh);
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
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
            objectsInUse.AddLast(ret);
            return info;
        }
        #endregion

        #region Refresh
        private bool GetLaneBeat(Vector2 pos, out int lane, out int[] beat)
        {
            lane = -1;
            beat = new int[3];
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, pos, Cam, out var point);

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
            int startBar = Mathf.Max(0, Mathf.FloorToInt(start / Editor.barHeight));
            int endBar = Mathf.Max(0, Mathf.FloorToInt(end / Editor.barHeight));
            for (int i = startBar; i <= endBar; i++)
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
                        CreateText(new Vector2(-20, y), i.ToString());
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
            foreach (var bpm in Chart.bpm)
            {
                float beat = ChartUtility.BeatToFloat(bpm.beat);
                if (beat < startBar || beat > endBar)
                    continue;

                float y = beat * Editor.barHeight - start;
                var info = CreateGridInfo(new Vector2(0, y), $"BPM:{Mathf.RoundToInt(bpm.value * 1000) / 1000f}", true);
                info.rect.sizeDelta = new Vector2(160, 40);
            }
            // Create timing point info
            foreach (var point in Group.points)
            {
                float beat = ChartUtility.BeatToFloat(point.beat);
                beat = Mathf.Max(0, beat);
                if (beat < startBar || beat > endBar)
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
            var ray = Cam.ScreenPointToRay(eventData.pressPosition);
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
                if (Editor.tool == EditorTool.Select)
                    return;
                if (eventData.button == PointerEventData.InputButton.Right || Editor.tool == EditorTool.Delete)
                {
                    Notes.UnselectAll();
                    return;
                }
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
                        cmds.Add(new ConnectNoteCmd(prev.note, note));
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
            else if (Editor.tool != EditorTool.Select)
            {
                Notes.UnselectAll();
            }
        }

        public TimingPoint GetTimingPointByBeat(float beatf)
        {
            foreach (var point in Group.points)
            {
                point.beatf = ChartUtility.BeatToFloat(point.beat);
                if (Mathf.Approximately(beatf, Mathf.Max(0, point.beatf)))
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
                            MessageBannerController.ShowMsg(LogLevel.INFO, "Cannot remove the very first timing point.");
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
                for (int i = 0; i < Group.points.Count; i++)
                {
                    var p = Group.points[i];
                    if (beatf < p.beatf)
                    {
                        Point = Group.points[i - 1].Copy();
                        break;
                    }
                    if (i == Group.points.Count - 1)
                    {
                        Point = Group.points[i].Copy();
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
            if (Editor.speedView)
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
            Editor.speedView = isOn;
            Core.onSpeedViewSwitched.Invoke();
        }
        #endregion
    }
}
