using UnityEngine;

namespace BGEditor
{
    public class RemoveNoteCmd : IEditorCmd
    {
        private Note note;

        public RemoveNoteCmd(Note note)
        {
            this.note = note;
        }

        public bool Commit(ChartCore core)
        {
            return core.RemoveNote(note);
        }

        public bool Rollback(ChartCore core)
        {
            return core.CreateNote(note);
        }
    }
}
