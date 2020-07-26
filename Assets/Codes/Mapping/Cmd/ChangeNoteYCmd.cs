using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class ChangeNoteYCmd : IEditorCmd
    {
        private V2.Note note;
        private float before;
        private float after;

        public ChangeNoteYCmd(V2.Note note, float y)
        {
            this.note = note;
            after = y;
        }

        public bool Commit(ChartCore core)
        {
            before = note.y;
            note.y = after;
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            note.y = before;
            core.onNoteModified.Invoke(note);
            return true;
        }
    }
}
