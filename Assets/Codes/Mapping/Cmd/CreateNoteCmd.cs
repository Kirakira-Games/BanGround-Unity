namespace BGEditor
{
    public class CreateNoteCmd : IEditorCmd
    {
        private Note note;
        private NoteType type;
        private int tickStack;

        public CreateNoteCmd(Note note)
        {
            this.note = note;
            type = note.type;
            tickStack = note.tickStack;
        }

        public bool Commit(ChartCore core)
        {
            note.type = type;
            note.tickStack = tickStack;
            return core.AddNote(note);
        }

        public bool Rollback(ChartCore core)
        {
            return core.RemoveNote(note);
        }
    }
}
