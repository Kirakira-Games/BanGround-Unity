using UnityEngine;

namespace BGEditor
{
    class DisconnectNoteCmdRaw : IEditorCmd
    {
        private V2.Note prev;
        private V2.Note next;
        private IEditNoteController Notes;

        public DisconnectNoteCmdRaw(IEditNoteController notes, V2.Note prev, V2.Note next)
        {
            Notes = notes;
            this.prev = prev;
            this.next = next;
        }

        public bool Commit(IChartCore core)
        {
            return Notes.ConnectNote(prev, null);
        }

        public bool Rollback(IChartCore core)
        {
            return Notes.ConnectNote(prev, next);
        }
    }

    public class DisconnectNoteCmd : CmdGroup
    {
        public DisconnectNoteCmd(IEditNoteController notes, V2.Note prev, V2.Note next)
        {
            Add(new DisconnectNoteCmdRaw(notes, prev, next));
            Add(new ChangeTickStackCmd(notes, next, notes.slideIdPool.RegisterNext()));
            Add(new AdjustSlideTickTypeCmd(notes, prev));
            Add(new AdjustSlideTickTypeCmd(notes, next));
        }
    }
}