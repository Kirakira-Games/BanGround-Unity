﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorLeftScrollBarController : CoreMonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Canvas canvas;
        public float PaddingBottom;
        public float PaddingTop;

        private float lastDown;
        private float DBCLICK_THRES = 0.5f;

        private HashSet<int> dbClick;
        private RectTransform rectTransform;
        private Rect rect;

        private void Seek(Vector2 pos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pos, Cam, out var point);
            float x = Mathf.InverseLerp(PaddingBottom, rectTransform.rect.height - PaddingTop, point.y);
            Core.SeekGrid(Mathf.Lerp(0, Editor.maxHeight, x));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dbClick.Contains(eventData.pointerId))
            {
                Seek(eventData.position);
            }
            else
            {
                Core.MoveGrid(-eventData.delta.y * (rectTransform.rect.height / rect.height));
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (lastDown >= Time.time - DBCLICK_THRES)
            {
                Seek(eventData.position);
                dbClick.Add(eventData.pointerId);
            }
            lastDown = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (dbClick.Contains(eventData.pointerId))
                dbClick.Remove(eventData.pointerId);
        }

        protected override void Awake()
        {
            base.Awake();
            rectTransform = GetComponent<RectTransform>();
            rect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
            dbClick = new HashSet<int>();
            lastDown = -1e5f;
        }
    }
}