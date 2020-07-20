using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering.UI;
using System.Linq;

namespace BGEditor
{
    public class EditNoteController : CoreMonoBehaviour
    {
        private Dictionary<V2.Note, EditorNoteBase> displayNotes = new Dictionary<V2.Note, EditorNoteBase>();
        public HashSet<EditorNoteBase> selectedNotes { get; private set; } = new HashSet<EditorNoteBase>();
        public IDPool slideIdPool { get; private set; } = new IDPool();

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

        private void Awake()
        {
            Core.onNoteCreated.AddListener(CreateNote);
            Core.onNoteRemoved.AddListener(RemoveNote);
            Core.onNoteModified.AddListener(ModifyNote);
            Core.onToolSwitched.AddListener(ToolSwitch);
        }

        public EditorNoteBase Find(V2.Note note)
        {
            return displayNotes[note];
        }

        public void CreateNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(!displayNotes.ContainsKey(note));
            EditorNoteBase notebase;
            if (note.tickStack > 0)
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

        public void RemoveNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(displayNotes.ContainsKey(note));
            Pool.Destroy(Find(note).gameObject);
            displayNotes.Remove(note);
        }

        public void ModifyNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            Debug.Assert(displayNotes.ContainsKey(note));
            var notebase = Find(note);
            notebase.UpdatePosition();
            notebase.Refresh();
        }

        public void SelectNote(V2.Note note)
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

        public void UnselectNote(V2.Note note)
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

        public bool ConnectNote(V2.Note prev, V2.Note next)
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
                note.Value.Refresh();
            }
        }

        public void ToolSwitch()
        {
            if (Editor.tool == EditorTool.Delete)
            {
                var tmpNotes = selectedNotes.ToArray();
                var cmd = new CmdGroup();
                foreach (var note in tmpNotes)
                    cmd.Add(new RemoveNoteCmd(note.note));
                var result = Core.Commit(cmd);
                Debug.Assert(result);
                selectedNotes.Clear();
            }
            else
            {
                UnselectAll();
            }
        }
    }
}
