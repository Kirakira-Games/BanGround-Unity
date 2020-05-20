using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

namespace BGEditor
{
    public class GridController : CoreMonoBehavior, IPointerClickHandler
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

        private bool GetTrackBeat(Vector2 pos, out int track, out int[] beat)
        {
            track = -1;
            beat = new int[3];
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, pos, Cam, out var point);

            // Compute beat
            float actualY = point.y + Editor.scrollPos - VPadding;
            float unitgrid = Editor.barHeight / Editor.gridDivision;
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
            track = Mathf.Clamp(Mathf.FloorToInt(point.x / laneWidth), 0, NoteUtility.LANE_COUNT - 1);
            return true;
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
            int endBar = Mathf.CeilToInt(end / Editor.barHeight);
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
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GetTrackBeat(eventData.pressPosition, out var track, out var beat))
            {
                Debug.Log($"{track}: {ChartUtility.ToString(beat)}");
            }
        }
    }
}
