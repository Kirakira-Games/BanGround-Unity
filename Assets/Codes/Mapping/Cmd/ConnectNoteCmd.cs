using NoteType = V2.NoteType;

namespace BGEditor
{
    class ConnectNoteCmdRaw : IEditorCmd
    {
        private V2.Note prev;
        private V2.Note next;
        private IEditNoteController Notes;

        public ConnectNoteCmdRaw(IEditNoteController notes, V2.Note prev, V2.Note next)
        {
            Notes = notes;
            this.prev = prev;
            this.next = next;
        }

        public bool Commit(IChartCore core)
        {
            return Notes.ConnectNote(prev, next);
        }

        public bool Rollback(IChartCore core)
        {
            return Notes.ConnectNote(prev, null);
        }
    }

    class AdjustSlideTickTypeCmd : IEditorCmd
    {
        private V2.Note note;
        private NoteType before;
        private IEditNoteController Notes;

        public AdjustSlideTickTypeCmd(IEditNoteController notes, V2.Note note)
        {
            Notes = notes;
            this.note = note;
            before = note.type;
        }

        public bool Commit(IChartCore core)
        {
            var notebase = Notes.Find(note) as EditorSlideNote;
            if (notebase.prev == null)
                note.type = NoteType.Single;
            else if (notebase.next == null)
                note.type = NoteType.SlideTickEnd;
            else
                note.type = NoteType.SlideTick;
            core.onNoteModified.Invoke(note);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            note.type = before;
            core.onNoteModified.Invoke(note);
            return true;
        }
    }

    public class ConnectNoteCmd : CmdGroup
    {
        public ConnectNoteCmd(IEditNoteController notes, V2.Note prev, V2.Note next)
        {
            Add(new ConnectNoteCmdRaw(notes, prev, next));
            Add(new ChangeTickStackCmd(notes, next, prev.tickStack));
            Add(new AdjustSlideTickTypeCmd(notes, prev));
            Add(new AdjustSlideTickTypeCmd(notes, next));
        }
    }
}
