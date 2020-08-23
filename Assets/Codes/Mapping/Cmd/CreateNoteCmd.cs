namespace BGEditor
{
    public class CreateNoteCmd : IEditorCmd
    {
        private V2.Note note;

        public CreateNoteCmd(V2.Note note)
        {
            this.note = note;
        }

        public bool Commit(IChartCore core)
        {
            return core.CreateNote(note);
        }

        public bool Rollback(IChartCore core)
        {
            return core.RemoveNote(note);
        }
    }
}
