using UnityEngine;

namespace BGEditor
{
    class DisconnectNoteCmdRaw : IEditorCmd
    {
        private V2.Note prev;
        private V2.Note next;

        public DisconnectNoteCmdRaw(V2.Note prev, V2.Note next)
        {
            this.prev = prev;
            this.next = next;
        }

        public bool Commit(ChartCore core)
        {
            return core.notes.ConnectNote(prev, null);
        }

        public bool Rollback(ChartCore core)
        {
            return core.notes.ConnectNote(prev, next);
        }
    }

    public class DisconnectNoteCmd : CmdGroup
    {
        public DisconnectNoteCmd(V2.Note prev, V2.Note next, EditNoteController controller)
        {
            Add(new DisconnectNoteCmdRaw(prev, next));
            Add(new ChangeTickStackCmd(next, controller.slideIdPool.RegisterNext()));
            Add(new AdjustSlideTickTypeCmd(prev));
            Add(new AdjustSlideTickTypeCmd(next));
        }
    }
}