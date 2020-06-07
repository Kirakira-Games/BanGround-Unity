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
        public int NumRows;
        public int PixelPerRow;
        public int PixelPerNote;

        private Texture2D texture;
        private Rect rect;
        private Color[] colorArray;
        private bool isAudioLoaded = false;
        private bool shouldApply = false;
        private float beatPerRow;
        private int[] numNotes;

        private void Start()
        {
            numNotes = new int[NumRows];
            rect = GetComponent<RectTransform>().rect;
            texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            var image = gameObject.AddComponent<Image>();
            image.material = new Material(image.material);
            image.material.mainTexture = texture;

            colorArray = texture.GetPixels32().Select(x => Color.clear).ToArray();
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
            beatPerRow = (float)Editor.numBeats / NumRows;
            for (int i = 0; i < numNotes.Length; i++)
                numNotes[i] = 0;
            foreach (var note in Chart.notes)
                CreateNote(note);
        }

        private void FillNote(int row, int num, Color color)
        {
            for (int i = PixelPerRow * row, ed = PixelPerRow * (row + 1); i < ed; i++)
            {
                for (int j = num * PixelPerNote, edj = (num + 1) * PixelPerNote; j < edj; j++)
                {
                    if (i < texture.height && j < texture.width)
                        texture.SetPixel(j, i, color);
                }
            }
        }

        private void CreateNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            int pos = Mathf.Clamp(Mathf.FloorToInt(ChartUtility.BeatToFloat(note.beat) / beatPerRow), 0, numNotes.Length - 1);
            FillNote(pos, numNotes[pos], FillColor);
            numNotes[pos]++;

            shouldApply = true;
        }

        private void RemoveNote(Note note)
        {
            if (!isAudioLoaded)
                return;

            int pos = Mathf.Clamp(Mathf.FloorToInt(ChartUtility.BeatToFloat(note.beat) / beatPerRow), 0, numNotes.Length - 1);
            numNotes[pos]--;
            FillNote(pos, numNotes[pos], Color.clear);

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
