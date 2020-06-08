using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorPreviewPanelController : CoreMonoBehaviour
    {
        public Color FillColor;
        public int NoteHalfHeight;
        public int NoteHalfWidth;

        private Texture2D texture;
        private Rect rect;
        private Color[] colorArray;
        private bool isAudioLoaded = false;
        private bool shouldApply = false;
        private float stackingAlpha;
        private int horizontalPadding;

        private void Start()
        {
            rect = GetComponent<RectTransform>().rect;
            texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            var image = gameObject.AddComponent<Image>();
            image.material = new Material(image.material);
            image.material.mainTexture = texture;
            horizontalPadding = (int)(rect.width - NoteHalfWidth * 2 * NoteUtility.LANE_COUNT) / (NoteUtility.LANE_COUNT + 1);

            // Colors
            stackingAlpha = FillColor.a;
            FillColor.a = 0;
            colorArray = texture.GetPixels32().Select(x => FillColor).ToArray();

            // Listeners
            Core.onNoteCreated.AddListener(CreateNote);
            Core.onNoteRemoved.AddListener(RemoveNote);
            if (Progress.audioLength > 0)
            {
                isAudioLoaded = true;
                Refresh();
            }
            Core.onAudioLoaded.AddListener(() => {
                isAudioLoaded = true;
                Refresh();
            });
            Core.onTimingModified.AddListener(Refresh);
        }

        private void Refresh()
        {
            texture.SetPixels(colorArray);
            foreach (var note in Chart.notes)
                CreateNote(note);
            shouldApply = true;
        }

        private void FillNote(float x, float y, float delta)
        {
            x = Mathf.Lerp(horizontalPadding + NoteHalfWidth, rect.width - NoteHalfWidth - horizontalPadding, x);
            y = Mathf.Lerp(NoteHalfHeight, rect.height - NoteHalfHeight, y);
            int xmin = Mathf.RoundToInt(x - NoteHalfWidth);
            int xmax = Mathf.RoundToInt(x + NoteHalfWidth);
            int ymin = Mathf.RoundToInt(y - NoteHalfHeight);
            int ymax = Mathf.RoundToInt(y + NoteHalfHeight);
            for (int i = xmin; i <= xmax; i++)
            {
                for (int j = ymin; j <= ymax; j++)
                {
                    var color = texture.GetPixel(i, j);
                    color.a += delta;
                    texture.SetPixel(i, j, color);
                }
            }
        }

        private void CreateNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            float row = Mathf.Clamp01(ChartUtility.BeatToFloat(note.beat) / Editor.numBeats);
            float col = Mathf.InverseLerp(0, NoteUtility.LANE_COUNT - 1, note.lane);
            FillNote(col, row, stackingAlpha);

            shouldApply = true;
        }

        private void RemoveNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            float row = Mathf.Clamp01(ChartUtility.BeatToFloat(note.beat) / Editor.numBeats);
            float col = Mathf.InverseLerp(0, NoteUtility.LANE_COUNT - 1, note.lane);
            FillNote(col, row, -stackingAlpha);

            shouldApply = true;
        }

        private void Update()
        {
            if (shouldApply)
            {
                shouldApply = false;
                texture.Apply();
            }
        }
    }
}
