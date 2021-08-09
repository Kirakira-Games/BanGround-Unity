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
        private IEditorInfo Editor;

        public RemoveTimingGroupCmd(IEditorInfo editor, int index)
        {
            Editor = editor;
            this.index = index;
        }

        public bool Commit(IChartCore core)
        {
            var groups = core.chart.groups;
            if (index < 0 || index >= groups.Count)
                return false;
            group = groups[index];

            core.chart.groups.RemoveAt(index);
            group.notes.ForEach(core.multinote.Remove);
            if (Editor.currentTimingGroup == core.chart.groups.Count)
                Editor.currentTimingGroup--;
            group.notes.ForEach(note => note.group = -1);
            core.AssignTimingGroups(core.chart);
            core.onTimingGroupModified.Invoke();
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            var groups = core.chart.groups;
            if (index == groups.Count)
                groups.Add(group);
            else
                groups.Insert(index, group);
            group.notes.ForEach(core.multinote.Put);
            Editor.currentTimingGroup = index;
            core.AssignTimingGroups(core.chart);
            core.onTimingGroupModified.Invoke();
            return true;
        }
    }
}
