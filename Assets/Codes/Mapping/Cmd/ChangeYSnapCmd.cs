using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class ChangeYSnapCmd : IEditorCmd
    {
        private int prev;
        private int target;
        private IEditorInfo Editor;

        public ChangeYSnapCmd(IEditorInfo editor, int target)
        {
            Editor = editor;
            this.target = target;
        }

        public bool Commit(IChartCore core)
        {
            prev = Editor.yDivision;
            core.SetYDivision(target);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            core.SetYDivision(prev);
            return true;
        }
    }
}
