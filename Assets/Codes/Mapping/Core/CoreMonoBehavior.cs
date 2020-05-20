using UnityEngine;

namespace BGEditor
{
    public class CoreMonoBehavior : MonoBehaviour
    {
        [HideInInspector]
        public ChartCore Core;
        public Chart Chart => Core.chart;
        public EditorInfo Editor => Core.editor;
        public ObjectPool Pool => Core.pool;
        public GridController Grid => Core.grid;
        public Camera Cam => Core.cam;
        public NoteController Notes => Core.notes;

        protected virtual void Awake()
        {
            Core = ChartCore.Instance;
        }
    }
}
