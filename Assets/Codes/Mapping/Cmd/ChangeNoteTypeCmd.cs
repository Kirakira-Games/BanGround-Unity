using NoteType = V2.NoteType;

namespace BGEditor
{
    class ChangeNoteTypeCmd : IEditorCmd
    {
        private V2.Note note;
        private NoteType before;
        private NoteType after;

        public ChangeNoteTypeCmd(V2.Note note, NoteType type)
        {
            this.note = note;
            before = note.type;
            after = type;
        }

        public bool Commit(IChartCore core)
        {
            note.type = after;
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            note.type = before;
            core.onNoteModified.Invoke(note);
            return true;
        }
    }
}
