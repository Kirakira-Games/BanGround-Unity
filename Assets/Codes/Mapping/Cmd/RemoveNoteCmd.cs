using UnityEngine;

namespace BGEditor
{
    public class RemoveNoteCmd : IEditorCmd
    {
        private V2.Note note;

        public RemoveNoteCmd(V2.Note note)
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
