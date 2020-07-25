using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class ChangeYLayerCmd : IEditorCmd
    {
        float prev;
        float target;

        public ChangeYLayerCmd(float target)
        {
            this.target = target;
        }

        public bool Commit(ChartCore core)
        {
            //core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            //core.onNoteModified.Invoke(note);
            return true;
        }
    }
}
