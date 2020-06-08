using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorPreviewPanelController : CoreMonoBehaviour
    {
        public Color[] FillColors;
        public int NoteHalfHeight;
        public int NoteHalfWidth;

        private Rect rect;
        private Texture2D[] textures;
        private Color[][] colorArray;
        private Image[] images;
        private bool isAudioLoaded = false;
        private bool shouldApply = false;
        private int horizontalPadding;

        private void Start()
        {
            rect = GetComponent<RectTransform>().rect;

            // Create Texture
            textures = new Texture2D[FillColors.Length];
            colorArray = new Color[FillColors.Length][];
            images = GetComponentsInChildren<Image>();
            for (int i = 0; i < FillColors.Length; i++)
            {
                textures[i] = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.ARGB32, false);
                images[i].material = new Material(images[i].material);
                images[i].material.mainTexture = textures[i];
                var color = FillColors[i];
                color.a = 0;
                colorArray[i] = textures[i].GetPixels32().Select(x => color).ToArray();
            }
            horizontalPadding = (int)(rect.width - NoteHalfWidth * 2 * NoteUtility.LANE_COUNT) / (NoteUtility.LANE_COUNT + 1);

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
            for (int i = 0; i < textures.Length; i++)
                textures[i].SetPixels(colorArray[i]);
            foreach (var note in Chart.notes)
                CreateNote(note);
            shouldApply = true;
        }

        private void FillNote(int id, float x, float y, float delta)
        {
            x = Mathf.Lerp(horizontalPadding + NoteHalfWidth, rect.width - NoteHalfWidth - horizontalPadding, x);
            y = Mathf.Lerp(NoteHalfHeight, rect.height - NoteHalfHeight, y);
            int xmin = Mathf.RoundToInt(x - NoteHalfWidth);
            int xmax = Mathf.RoundToInt(x + NoteHalfWidth);
            int ymin = Mathf.RoundToInt(y - NoteHalfHeight);
            int ymax = Mathf.RoundToInt(y + NoteHalfHeight);
            for (int i = xmin; i < xmax; i++)
            {
                for (int j = ymin; j < ymax; j++)
                {
                    var color = textures[id].GetPixel(i, j);
                    color.a += delta;
                    textures[id].SetPixel(i, j, color);
                }
            }
        }

        private static int GetIdByNote(Note note)
        {
            if (note.tickStack != -1)
                return 2;
            return note.type == NoteType.Single ? 0 : 1;
        }

        private void CreateNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            float row = Mathf.Clamp01(ChartUtility.BeatToFloat(note.beat) / Editor.numBeats);
            float col = Mathf.InverseLerp(0, NoteUtility.LANE_COUNT - 1, note.lane);
            int id = GetIdByNote(note);
            FillNote(id, col, row, FillColors[id].a);

            shouldApply = true;
        }

        private void RemoveNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            float row = Mathf.Clamp01(ChartUtility.BeatToFloat(note.beat) / Editor.numBeats);
            float col = Mathf.InverseLerp(0, NoteUtility.LANE_COUNT - 1, note.lane);
            int id = GetIdByNote(note);
            FillNote(id, col, row, -FillColors[id].a);

            shouldApply = true;
        }

        private void Update()
        {
            if (shouldApply)
            {
                shouldApply = false;
                foreach (var texture in textures)
                    texture.Apply();
            }
        }
    }
}
