using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class RemoveTimingGroupCmd : IEditorCmd
    {
        private int index;
        private V2.TimingGroup group;

        public RemoveTimingGroupCmd(int index)
        {
            this.index = index;
        }

        public bool Commit(ChartCore core)
        {
            var groups = core.chart.groups;
            if (index < 0 || index >= groups.Count)
                return false;
            group = groups[index];

            core.chart.groups.RemoveAt(index);
            group.notes.ForEach(core.multinote.Remove);
            if (core.editor.currentTimingGroup == core.chart.groups.Count)
                core.editor.currentTimingGroup--;
            group.notes.ForEach(note => note.group = -1);
            ChartCore.AssignTimingGroups(core.chart);
            core.onTimingGroupModified.Invoke();
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            var groups = core.chart.groups;
            if (index == groups.Count)
                groups.Add(group);
            else
                groups.Insert(index, group);
            group.notes.ForEach(core.multinote.Put);
            core.editor.currentTimingGroup = index;
            ChartCore.AssignTimingGroups(core.chart);
            core.onTimingGroupModified.Invoke();
            return true;
        }
    }
}
