
namespace BGEditor
{
    class ChangeYLayerCmd : IEditorCmd
    {
        private float prev;
        private float target;
        private V2.Note[] selected;
        private IEditNoteController Notes;
        private IEditorInfo Editor;

        public ChangeYLayerCmd(IEditNoteController notes, IEditorInfo editor, float target)
        {
            Notes = notes;
            Editor = editor;
            this.target = target;
        }

        public bool Commit(IChartCore core)
        {
            prev = Editor.yPos;
            selected = Notes.GetSelectedNotes();
            core.SetY(target);
            Notes.MoveY(selected, target);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            core.SetY(prev);
            Notes.MoveY(selected, prev);
            return true;
        }
    }
}
