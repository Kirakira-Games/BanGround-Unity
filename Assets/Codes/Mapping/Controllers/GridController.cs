using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking.NetworkSystem;
using System;

namespace BGEditor
{
    public class GridController : CoreMonoBehaviour, IPointerClickHandler
    {
        public float VPadding;
        public float LineBoundingHeight;

        private LinkedList<GameObject> objectsInUse;
        private float gridHeight;
        private float gridWidth;
        float laneWidth;
        private RectTransform parentRect;

        private void Start()
        {
            objectsInUse = new LinkedList<GameObject>();
            parentRect = GetComponent<RectTransform>();

            Core.onGridModifed.AddListener(Refresh);
            Core.onGridMoved.AddListener(Refresh);
            var rect = GetComponent<RectTransform>().rect;
            gridHeight = rect.height;
            gridWidth = rect.width;
            laneWidth = gridWidth / 7;

            Refresh();
        }

        private GameObject CreateLine(float width, Vector2 anchoredPosition, Color color)
        {
            var ret = Pool.Create<Image>();
            ret.transform.SetParent(transform, false);
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

        private GameObject CreateText(Vector2 anchoredPosition, string text)
        {
            var ret = Pool.Create<Text>();
            ret.transform.SetParent(transform, false);
            var rect = ret.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.anchoredPosition = anchoredPosition;
            var txt = rect.GetComponent<Text>();
            txt.raycastTarget = false;
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 48;
            objectsInUse.AddLast(ret);
            return ret;
        }

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

        public Vector2 GetLocalPosition(int lane, float beat)
        {
            float x = (lane + 0.5f) * laneWidth;
            float y = beat * Editor.barHeight - (Editor.scrollPos - VPadding);
            return new Vector2(x, y);
        }

        public Vector2 GetLocalPosition(int lane, int[] beat)
        {
            float floatbeat = ChartUtility.BeatToFloat(beat);
            return GetLocalPosition(lane, floatbeat);
        }

        public void Refresh()
        {
            foreach (var obj in objectsInUse)
            {
                Pool.Destroy(obj);
            }
            objectsInUse.Clear();

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
                        color = Color.white;
                        CreateText(new Vector2(-50, y), i.ToString());
                    }
                    else if (j * 2 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Color.cyan;
                    }
                    else if (j * 3 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Color.yellow;
                    }
                    else if (j * 4 % Editor.gridDivision == 0)
                    {
                        width = 2;
                        color = new Color(1, 0.5f, 0);
                    }
                    else
                    {
                        width = 2;
                        color = Color.gray;
                    }
                    var obj = CreateLine(width, new Vector2(0, y), color);
                }
            }

            Notes.Refresh();
        }

        public void OnPointerClick(PointerEventData eventData)
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
                        body.parent.OnPointerClick(eventData);
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
                var note = new Note
                {
                    lane = lane,
                    beat = beat,
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
    }
}
