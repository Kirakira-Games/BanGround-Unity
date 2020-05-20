using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BGEditor
{
    public class GridController : CoreMonoBehavior
    {
        public const float VPADDING = 60;

        private LinkedList<GameObject> objectsInUse;
        private float gridHeight;
        private float gridWidth;

        private void Start()
        {
            objectsInUse = new LinkedList<GameObject>();

            Core.onGridModifed.AddListener(Refresh);
            Core.onGridMoved.AddListener(Refresh);
            var rect = GetComponent<RectTransform>().rect;
            gridHeight = rect.height;
            gridWidth = rect.width;

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
            return ret;
        }

        public void Refresh()
        {
            foreach (var obj in objectsInUse)
            {
                Pool.Destroy(obj);
            }
            objectsInUse.Clear();

            float start = Editor.scrollPos - VPADDING;
            float end = Editor.scrollPos - VPADDING + gridHeight;
            int startBar = Mathf.FloorToInt(start / Editor.barHeight);
            int endBar = Mathf.CeilToInt(end / Editor.barHeight);
            for (int i = startBar; i <= endBar; i++)
            {
                for (int j = 0; j < Editor.gridDivision; j++)
                {
                    float y = Mathf.Round(((float)j / Editor.gridDivision + i) * Editor.barHeight - Editor.scrollPos);
                    float width;
                    Color color;
                    if (j == 0)
                    {
                        width = 3;
                        color = Color.white;
                    }
                    else if (j * 2 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Color.red;
                    }
                    else if (j * 3 % Editor.gridDivision == 0)
                    {
                        width = 3;
                        color = Color.green;
                    }
                    else if (j * 4 % Editor.gridDivision == 0)
                    {
                        width = 2;
                        color = Color.blue;
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
    }
}
