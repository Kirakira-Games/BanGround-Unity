namespace BGEditor
{
    public class RemoveNoteCmd : IEditorCmd
    {
        private Note note;
        private NoteType type;
        private int tickStack;

        public RemoveNoteCmd(Note note)
        {
            this.note = note;
            type = note.type;
            tickStack = note.tickStack;
        }

        public bool Commit(ChartCore core)
        {
            note.type = type;
            note.tickStack = tickStack;
            return core.RemoveNote(note);
        }

        public bool Rollback(ChartCore core)
        {
            return core.AddNote(note);
        }
    }
}
