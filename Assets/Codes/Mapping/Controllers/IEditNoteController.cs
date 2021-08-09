using System.Collections.Generic;

namespace BGEditor
{
    public interface IEditNoteController
    {
        HashSet<EditorNoteBase> selectedNotes { get; }
        EditorSlideNote singleSlideSelected { get; }
        IDPool slideIdPool { get; }

        bool ConnectNote(V2.Note prev, V2.Note next);
        void CreateNote(V2.Note note);
        EditorNoteBase Find(V2.Note note);
        V2.Note[] GetSelectedNotes();
        void ModifyNote(V2.Note note);
        void MoveY(IEnumerable<V2.Note> notes, float target);
        void MoveY(V2.Note note, float target);
        void Refresh();
        void RemoveAllSelected();
        void RemoveNote(V2.Note note);
        void SelectNote(EditorNoteBase note);
        void SelectNote(V2.Note note);
        void ToolSwitch();
        void UnselectAll();
        void UnselectNote(EditorNoteBase note);
        void UnselectNote(V2.Note note);
        void YSnapChange(int prev, int div);
    }
}
