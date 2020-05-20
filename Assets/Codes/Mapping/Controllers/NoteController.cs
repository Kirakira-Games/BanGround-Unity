namespace BGEditor
{
    public class NoteController : CoreMonoBehavior
    {
        private void Start()
        {
            Core.onGridModifed.AddListener(Refresh);
            Core.onGridMoved.AddListener(Refresh);
            Core.onNoteCreated.AddListener(CreateNote);
            Core.onNoteRemoved.AddListener(RemoveNote);
            Core.onNoteModified.AddListener(ModifyNote);
        }

        public void CreateNote(Note note)
        {

        }

        public void RemoveNote(Note note)
        {

        }

        public void ModifyNote(Note note)
        {

        }

        public void Refresh()
        {

        }
    }
}
