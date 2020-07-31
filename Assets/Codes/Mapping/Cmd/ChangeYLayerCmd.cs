
namespace BGEditor
{
    class ChangeYLayerCmd : IEditorCmd
    {
        float prev;
        float target;
        V2.Note[] selected;

        public ChangeYLayerCmd(float target)
        {
            this.target = target;
        }

        public bool Commit(ChartCore core)
        {
            prev = core.editor.yPos;
            selected = core.notes.GetSelectedNotes();
            core.SetY(target);
            core.notes.MoveY(selected, target);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            core.SetY(prev);
            core.notes.MoveY(selected, prev);
            return true;
        }
    }
}
