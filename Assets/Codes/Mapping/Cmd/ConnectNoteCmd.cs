namespace BGEditor
{
    class ConnectNoteCmdRaw : IEditorCmd
    {
        private Note prev;
        private Note next;

        public ConnectNoteCmdRaw(Note prev, Note next)
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

    class AdjustSlideTickTypeCmd : IEditorCmd
    {
        private Note note;
        private NoteType before;

        public AdjustSlideTickTypeCmd(Note note)
        {
            this.note = note;
            before = note.type;
        }

        public bool Commit(ChartCore core)
        {
            var notebase = core.notes.Find(note) as EditorSlideNote;
            if (notebase.prev == null)
                note.type = NoteType.Single;
            else if (notebase.next == null)
                note.type = NoteType.SlideTickEnd;
            else
                note.type = NoteType.SlideTick;
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(ChartCore core)
        {
            note.type = before;
            core.onNoteModified.Invoke(note);
            return true;
        }
    }

    public class ConnectNoteCmd : CmdGroup
    {
        public ConnectNoteCmd(Note prev, Note next)
        {
            Add(new ConnectNoteCmdRaw(prev, next));
            Add(new ChangeTickStackCmd(next, prev.tickStack));
            Add(new AdjustSlideTickTypeCmd(prev));
            Add(new AdjustSlideTickTypeCmd(next));
        }
    }
}
