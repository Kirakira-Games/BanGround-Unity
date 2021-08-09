using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class AddTimingPointCmd : IEditorCmd
    {
        private V2.TimingPoint point;
        private V2.TimingGroup group;

        public AddTimingPointCmd(V2.TimingPoint point)
        {
            this.point = point;
        }

        public bool Commit(IChartCore core)
        {
            group = core.group;
            float beat = ChartUtility.BeatToFloat(point.beat);
            for (int i = 0; i < group.points.Count; i++)
            {
                float pbeat = ChartUtility.BeatToFloat(group.points[i].beat);
                if (beat < pbeat)
                {
                    group.points.Insert(i, point);
                    break;
                }
                if (i == group.points.Count - 1)
                {
                    group.points.Add(point);
                    break;
                }
            }
            core.onTimingPointModified.Invoke();
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            group.points.Remove(point);
            core.onTimingPointModified.Invoke();
            return true;
        }
    }
}
