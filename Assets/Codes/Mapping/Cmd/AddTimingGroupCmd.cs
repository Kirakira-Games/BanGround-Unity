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
        public bool Commit(ChartCore core)
        {
            core.chart.groups.Add(V2.TimingGroup.Default());
            core.editor.currentTimingGroup = core.chart.groups.Count - 1;
            core.onTimingGroupModified.Invoke();
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            if (core.chart.groups.Count <= 1)
                return false;
            core.chart.groups.RemoveAt(core.chart.groups.Count - 1);
            if (core.editor.currentTimingGroup == core.chart.groups.Count)
                core.editor.currentTimingGroup--;
            core.onTimingGroupModified.Invoke();
            return true;
        }
    }
}
