using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace BGEditor
{
    public class GridInfoText : MonoBehaviour
    {
        public Text txt;
        public Image img;

        [HideInInspector]
        public RectTransform rect;

        public static readonly Color BG_COLOR = new Color(0, 0, 0, 0.5f);

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void ResetLeft(Vector2 anchoredPosition, string text)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = anchoredPosition;
            txt.text = text;
        }

        public void ResetRight(Vector2 anchoredPosition, string text)
        {
            rect.anchorMin = Vector2.right;
            rect.anchorMax = Vector2.right;
            rect.pivot = Vector2.right;
            rect.anchoredPosition = anchoredPosition;
            txt.text = text;
        }
    }
}