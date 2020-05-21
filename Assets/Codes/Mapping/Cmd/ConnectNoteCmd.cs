using System;

namespace BGEditor
{
    public class ConnectNoteCmd : IEditorCmd
    {
        private Note prev;
        private Note next;

        public ConnectNoteCmd(Note prev, Note next)
        {
            this.prev = prev;
            this.next = next;
        }

        public bool Commit(ChartCore core)
        {
            return core.notes.ConnectNote(prev, next);
        }

        public bool Rollback(ChartCore core)
        {
            return core.notes.ConnectNote(prev, null);
        }
    }
}
