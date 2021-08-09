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

        public bool Commit(IChartCore core)
        {
            return core.RemoveNote(note);
        }

        public bool Rollback(IChartCore core)
        {
            return core.CreateNote(note);
        }
    }
}
