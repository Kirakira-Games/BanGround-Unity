using UnityEngine;

namespace BGEditor
{
    class ChangeNoteYCmd : IEditorCmd
    {
        private V2.Note note;
        private float before;
        private float after;
        private IEditNoteController Notes;

        public ChangeNoteYCmd(IEditNoteController notes, V2.Note note, float y)
        {
            Notes = notes;
            this.note = note;
            after = y;
        }

        public bool Commit(IChartCore core)
        {
            before = note.yOrNaN;
            Notes.MoveY(note, after);
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            Notes.MoveY(note, before);
            core.onNoteModified.Invoke(note);
            return true;
        }
    }
}
