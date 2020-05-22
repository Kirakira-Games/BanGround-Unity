using UnityEngine;

namespace BGEditor
{
    public class CoreMonoBehaviour : MonoBehaviour
    {
        public ChartCore Core => ChartCore.Instance;
        public Chart Chart => Core.chart;
        public EditorInfo Editor => Core.editor;
        public ObjectPool Pool => Core.pool;
        public GridController Grid => Core.grid;
        public Camera Cam => Core.cam;
        public EditNoteController Notes => Core.notes;
        public TimingController Timing => Core.timing;
    }
}
