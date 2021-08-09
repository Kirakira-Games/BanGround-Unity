using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGEditor
{
    class ChangeTickStackCmd : IEditorCmd
    {
        private V2.Note note;
        private int before;
        private int after;
        private IEditNoteController Notes;

        public ChangeTickStackCmd(IEditNoteController notes, V2.Note note, int tickstack)
        {
            Notes = notes;
            this.note = note;
            before = note.tickStack;
            after = tickstack;
        }

        public bool Commit(IChartCore core)
        {
            var slide = Notes.Find(note) as EditorSlideNote;
            while (slide.prev != null)
                slide = slide.prev;
            slide.SetTickstack(after);
            return true;
        }

        public bool Rollback(IChartCore core)
        {
            var slide = Notes.Find(note) as EditorSlideNote;
            while (slide.prev != null)
                slide = slide.prev;
            slide.SetTickstack(before);
            return true;
        }
    }
}
