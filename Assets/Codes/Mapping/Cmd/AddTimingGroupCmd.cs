using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace BGEditor
{
    class AddTimingGroupCmd : IEditorCmd
    {
        private IEditorInfo Editor;

        public AddTimingGroupCmd(IEditorInfo editor)
        {
            Editor = editor;
        }

        public bool Commit(IChartCore core)
        {
            core.chart.groups.Add(V2.TimingGroup.Default());
            Editor.currentTimingGroup = core.chart.groups.Count - 1;
            core.onTimingGroupModified.Invoke();
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            if (core.chart.groups.Count <= 1)
                return false;
            core.chart.groups.RemoveAt(core.chart.groups.Count - 1);
            if (Editor.currentTimingGroup == core.chart.groups.Count)
                Editor.currentTimingGroup--;
            core.onTimingGroupModified.Invoke();
            return true;
        }
    }
}
