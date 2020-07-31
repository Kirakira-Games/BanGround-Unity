using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering.UI;
using System.Linq;
using XLua;

namespace BGEditor
{
    public class EditNoteController : CoreMonoBehaviour
    {
        private Dictionary<V2.Note, EditorNoteBase> displayNotes = new Dictionary<V2.Note, EditorNoteBase>();
        private Dictionary<int, HashSet<EditorNoteBase>> notesByBeat = new Dictionary<int, HashSet<EditorNoteBase>>();
        private List<EditorNoteBase> updateList = new List<EditorNoteBase>();
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
            Core.onYSnapModified.AddListener(YSnapChange);
            Core.onYFilterSwitched.AddListener(Refresh);
            Core.onSpeedViewSwitched.AddListener(() =>
            {
                UnselectAll();
            });
            Core.onTimingGroupSwitched.AddListener(() =>
            {
                UnselectAll();
                Refresh();
            });
        }

        public EditorNoteBase Find(V2.Note note)
        {
            var set = notesByBeat[note.beat[0]];
            foreach (var notebase in set)
            {
                if (ReferenceEquals(notebase.note, note))
                {
                    return notebase;
                }
            }
            return null;
        }

        public void CreateNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
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
            // Add notes to dict
            displayNotes.Add(note, notebase);
            if (!notesByBeat.ContainsKey(note.beat[0]))
                notesByBeat.Add(note.beat[0], new HashSet<EditorNoteBase> { notebase });
            else
                notesByBeat[note.beat[0]].Add(notebase);
            notebase.Init(note);
            notebase.Refresh();
            (notebase as EditorSlideNote)?.UpdateBodyMesh();
        }

        public void RemoveNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            var notebase = Find(note);
            Core.tooltip.Hide(notebase);
            Pool.Destroy(notebase.gameObject);
            displayNotes.Remove(note);
            bool result = notesByBeat[note.beat[0]].Remove(notebase);
            Debug.Assert(result);
        }

        public void ModifyNote(V2.Note note)
        {
            if (note.type == NoteType.BPM)
                return;
            var notebase = Find(note);
            Debug.Assert(notebase != null);
            notebase.Refresh();
            (notebase as EditorSlideNote)?.UpdateBodyMesh();
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
            var note = Find(prev) as EditorSlideNote;
            Debug.Assert(note != null);
            if (next == null)
            {
                var nxt = note.next;
                if (!note.SetNext(null))
                    return false;
                for (int i = prev.beat[0] + 1; i <= nxt.note.beat[0]; i++)
                {
                    bool ret = notesByBeat[i].Remove(note);
                    ret &= notesByBeat[i-1].Remove(nxt);
                    Debug.Assert(ret);
                }
                return true;
            }
            else
            {
                var nxt = Find(next) as EditorSlideNote;
                if (!note.SetNext(nxt))
                    return false;
                for (int i = prev.beat[0] + 1; i <= next.beat[0]; i++)
                {
                    if (!notesByBeat.ContainsKey(i))
                        notesByBeat.Add(i, new HashSet<EditorNoteBase> { note });
                    else
                        notesByBeat[i].Add(note);
                    notesByBeat[i - 1].Add(nxt);
                }
                return true;
            }
        }

        public void Refresh()
        {
            if (displayNotes == null)
            {
                // Have not initialized
                return;
            }
            var notes = displayNotes.Values.ToArray();
            foreach (var note in notes)
            {
                note.Refresh();
            }
            foreach (var note in notes)
            {
                (note as EditorSlideNote)?.UpdateBodyMesh();
            }

            displayNotes.Clear();
            updateList.Clear();
            for (int i = Grid.StartBar; i <= Grid.EndBar; i++)
            {
                if (!notesByBeat.ContainsKey(i))
                    continue;
                foreach (var note in notesByBeat[i])
                {
                    if (displayNotes.ContainsKey(note.note) || note.note.group != Editor.currentTimingGroup)
                        continue;
                    displayNotes.Add(note.note, note);
                    if (!note.gameObject.activeSelf)
                    {
                        updateList.Add(note);
                    }
                }
            }
            updateList.ForEach(note => note.Refresh());
            updateList.ForEach(note => (note as EditorSlideNote)?.UpdateBodyMesh());
        }

        public void RemoveAllSelected()
        {
            var tmpNotes = selectedNotes.ToArray();
            var cmd = new CmdGroup();
            foreach (var note in tmpNotes)
                cmd.Add(new RemoveNoteCmd(note.note));
            var result = Core.Commit(cmd);
            Debug.Assert(result);
            selectedNotes.Clear();
        }

        public void ToolSwitch()
        {
            if (Editor.tool == EditorTool.Delete)
            {
                RemoveAllSelected();
            }
            else
            {
                UnselectAll();
            }
        }

        public V2.Note[] GetSelectedNotes()
        {
            var ret = new List<V2.Note>();
            foreach (var note in selectedNotes)
            {
                ret.Add(note.note);
                var slideNote = note as EditorSlideNote;
                if (slideNote != null)
                {
                    while (slideNote.prev != null)
                    {
                        slideNote = slideNote.prev;
                        ret.Add(slideNote.note);
                    }
                }
            }
            return ret.ToArray();
        }

        public void YSnapChange(int prev, int div)
        {
            if (prev == 0 || div == 0)
            {
                MoveY(GetSelectedNotes(), div == 0 ? float.NaN : 0);
            }
        }

        public void MoveY(V2.Note note, float target)
        {
            if (float.IsNaN(target))
            {
                note.y = 0;
                if (note.lane == -1)
                {
                    note.lane = Mathf.Clamp(Mathf.RoundToInt(note.x), 0, NoteUtility.LANE_COUNT - 1);
                    note.x = 0;
                }
            }
            else
            {
                note.y = target;
                if (note.lane != -1)
                {
                    note.x = note.lane;
                    note.lane = -1;
                }
            }
        }

        public void MoveY(IEnumerable<V2.Note> notes, float target)
        {
            foreach (var note in notes)
            {
                MoveY(note, target);
            }
            Refresh();
        }
    }
}
