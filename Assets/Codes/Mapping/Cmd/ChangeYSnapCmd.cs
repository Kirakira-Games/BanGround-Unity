using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class ChangeYSnapCmd : IEditorCmd
    {
        int prev;
        int target;

        public ChangeYSnapCmd(int target)
        {
            this.target = target;
        }

        public bool Commit(ChartCore core)
        {
            prev = core.editor.yDivision;
            core.SetYDivision(target);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            core.SetYDivision(prev);
            return true;
        }
    }
}
