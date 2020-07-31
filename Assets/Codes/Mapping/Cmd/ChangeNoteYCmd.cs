using UnityEngine;

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
            before = note.yOrNaN;
            core.notes.MoveY(note, after);
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            core.notes.MoveY(note, before);
            core.onNoteModified.Invoke(note);
            return true;
        }
    }
}
