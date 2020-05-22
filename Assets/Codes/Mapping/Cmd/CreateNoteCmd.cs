namespace BGEditor
{
    public class CreateNoteCmd : IEditorCmd
    {
        private Note note;

        public CreateNoteCmd(Note note)
        {
            this.note = note;
        }

        public bool Commit(ChartCore core)
        {
            return core.CreateNote(note);
        }

        public bool Rollback(ChartCore core)
        {
            return core.RemoveNote(note);
        }
    }
}
