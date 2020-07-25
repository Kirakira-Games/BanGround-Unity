﻿using UnityEngine;

namespace BGEditor
{
    public class CoreMonoBehaviour : MonoBehaviour
    {
        public static ChartCore Core => ChartCore.Instance;
        public static V2.Chart Chart => Core.chart;
        public static V2.TimingGroup Group => Core.group;
        public static EditorInfo Editor => Core.editor;
        public static ObjectPool Pool => Core.pool;
        public static GridController Grid => Core.grid;
        public static Camera Cam => Core.cam;
        public static EditNoteController Notes => Core.notes;
        public static TimingController Timing => Core.timing;
        public static AudioProgressController Progress => Core.progress;
    }
}
