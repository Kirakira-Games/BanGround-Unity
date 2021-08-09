using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class RemoveTimingPointCmd : IEditorCmd
    {
        private V2.TimingPoint point;
        private V2.TimingGroup group;
        private int index = -1;

        public RemoveTimingPointCmd(V2.TimingPoint point)
        {
            this.point = point;
        }

        public bool Commit(IChartCore core)
        {
            group = core.group;
            index = group.points.IndexOf(point);
            if (index < 0) return false;
            group.points.RemoveAt(index);
            core.onTimingPointModified.Invoke();
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            if (index < 0) return false;
            if (index >= group.points.Count)
                group.points.Add(point);
            else
                group.points.Insert(index, point);
            core.onTimingPointModified.Invoke();
            return true;
        }
    }
}
