using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering.UI;
using System.Linq;

namespace BGEditor
{
    public class EditNoteController : CoreMonoBehaviour
    {
        private Dictionary<Note, EditorNoteBase> displayNotes;
        public HashSet<EditorNoteBase> selectedNotes { get; private set; }
        public IDPool slideIdPool { get; private set; }

        public EditorSlideNote singleSlideSelected {
            get
            {
                if (selectedNotes.Count != 1)
                    return null;
                var note = selectedNotes.GetEnumerator();
                note.MoveNext();
                return note.Current as EditorSlideNote;
            }
        }

        private void Start()
        {
            displayNotes = new Dictionary<Note, EditorNoteBase>();
            selectedNotes = new HashSet<EditorNoteBase>();
            slideIdPool = new IDPool();
            Core.onNoteCreated.AddListener(CreateNote);
            Core.onNoteRemoved.AddListener(RemoveNote);
            Core.onNoteModified.AddListener(ModifyNote);
            Core.onToolSwitched.AddListener(ToolSwitch);
        }

        public EditorNoteBase Find(Note note)
        {
            return displayNotes[note];
        }

        public void CreateNote(Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(!displayNotes.ContainsKey(note));
            EditorNoteBase notebase;
            if (note.tickStack != -1)
            {
                notebase = Pool.Create<EditorSlideNote>().GetComponent<EditorSlideNote>();
            }
            else if (note.type == NoteType.Single)
            {
                notebase = Pool.Create<EditorSingleNote>().GetComponent<EditorSingleNote>();
            }
            else if (note.type == NoteType.Flick)
            {
                notebase = Pool.Create<EditorFlickNote>().GetComponent<EditorFlickNote>();
            }
            else
            {
                throw new NotImplementedException($"NoteType {Enum.GetName(typeof(NoteType), note.type)} is unsupported.");
            }
            displayNotes.Add(note, notebase);
            notebase.Init(note);
        }

        public void RemoveNote(Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(displayNotes.ContainsKey(note));
            Pool.Destroy(Find(note).gameObject);
            displayNotes.Remove(note);
        }

        public void ModifyNote(Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(displayNotes.ContainsKey(note));
            var notebase = Find(note);
            notebase.UpdatePosition();
            notebase.Refresh();
        }

        public void SelectNote(Note note)
        {
            SelectNote(Find(note));
        }

        public void SelectNote(EditorNoteBase note)
        {
            if (selectedNotes.Contains(note))
                return;
            selectedNotes.Add(note);
            note.Select();
        }

        public void UnselectNote(Note note)
        {
            UnselectNote(Find(note));
        }

        public void UnselectNote(EditorNoteBase note)
        {
            if (!selectedNotes.Contains(note))
                return;
            selectedNotes.Remove(note);
            note.Unselect();
        }

        public void UnselectAll()
        {
            foreach (var note in selectedNotes)
            {
                note.Unselect();
            }
            selectedNotes.Clear();
        }

        public bool ConnectNote(Note prev, Note next)
        {
            Debug.Assert(displayNotes.ContainsKey(prev));
            var note = Find(prev) as EditorSlideNote;
            if (next == null)
                return note.SetNext(null);
            else
                return note.SetNext(Find(next) as EditorSlideNote);
        }

        public void Refresh()
        {
            if (displayNotes == null)
            {
                // Have not initialized
                return;
            }
            foreach (var note in displayNotes)
            {
                note.Value.UpdatePosition();
            }
            foreach (var note in displayNotes)
            {
                (note.Value as EditorSlideNote)?.Refresh();
            }
        }

        public void ToolSwitch()
        {
            if (Editor.tool == EditorTool.Delete)
            {
                var tmpNotes = selectedNotes.ToArray();
                foreach (var note in tmpNotes)
                    note.Remove();
                selectedNotes.Clear();
            }
            else
            {
                UnselectAll();
            }
        }
    }
}
