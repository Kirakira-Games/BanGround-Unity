using UnityEngine;

namespace BGEditor
{
    public class CoreMonoBehavior : MonoBehaviour
    {
        protected ChartCore Core;
        protected Chart Chart => Core.chart;
        protected EditorInfo Editor => Core.editor;

        protected virtual void Awake()
        {
            Core = ChartCore.Instance;
        }
    }
}
