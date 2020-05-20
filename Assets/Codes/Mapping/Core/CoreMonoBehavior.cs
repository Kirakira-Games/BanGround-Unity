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

        protected virtual void Awake()
        {
            Core = ChartCore.Instance;
        }
    }
}
