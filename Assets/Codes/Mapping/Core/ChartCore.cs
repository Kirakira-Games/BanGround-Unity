using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BGEditor
{
    public class NoteEvent : UnityEvent<Note> { }

    public class ChartCore : MonoBehaviour
    {
        public static ChartCore Instance;

        [HideInInspector]
        public Chart chart { get; private set; }

        public UnityEvent onTimingModified;
        public UnityEvent onGridMoved;
        public UnityEvent onGridModifed;
        public NoteEvent onNoteCreated;
        public NoteEvent onNoteModified;
        public NoteEvent onNoteRemoved;

        private LinkedList<IEditorCmd> cmdList;
        private LinkedListNode<IEditorCmd> lastCmd;
        private const int MAX_UNDO_COUNT = 100;

        public Dictionary<NotePosition, Note> groundNotes;
        public EditorInfo editor { get; private set; }

        void Awake()
        {
            Instance = this;

            // Initialize events
            onTimingModified = new UnityEvent();
            onGridMoved = new UnityEvent();
            onGridModifed = new UnityEvent();
            onNoteCreated = new NoteEvent();
            onNoteModified = new NoteEvent();
            onNoteRemoved = new NoteEvent();

            // Initialize commands
            cmdList = new LinkedList<IEditorCmd>();
            lastCmd = cmdList.AddLast(new FailCommand());

            // Initialize ground notes
            groundNotes = new Dictionary<NotePosition, Note>();

            // Initialize editor info
            editor = new EditorInfo();
        }

        /// <summary>
        /// Commit an editor command.
        /// </summary>
        /// <returns>Whether the commit is successful.</returns>
        public bool Commit(IEditorCmd cmd)
        {
            if (!cmd.Commit(this))
                return false;
            while (lastCmd.Next != null)
                cmdList.RemoveLast();
            lastCmd = cmdList.AddLast(cmd);
            while (cmdList.Count > MAX_UNDO_COUNT)
                cmdList.Remove(cmdList.First.Next);
            return true;
        }


        /// <summary>
        /// Undo the last editor command.
        /// </summary>
        /// <returns>Whether the rollback is successful.</returns>
        public bool Rollback()
        {
            if (!lastCmd.Value.Rollback(this))
                return false;
            lastCmd = lastCmd.Previous;
            return true;
        }


        /// <summary>
        /// Redo the last editor command.
        /// </summary>
        /// <returns>Whether the redo is successful.</returns>
        public bool Redo()
        {
            if (lastCmd.Next == null || !lastCmd.Next.Value.Commit(this))
                return false;
            lastCmd = lastCmd.Next;
            return true;
        }

        public bool AddNote(Note note)
        {
            if (!ChartUtility.IsFuwafuwa(note))
            {
                var pos = ChartUtility.GetPosition(note);
                if (groundNotes.ContainsKey(pos))
                    return false;
                groundNotes.Add(pos, note);
            }
            chart.notes.Add(note);
            onNoteCreated.Invoke(note);
            return true;
        }

        public Note GetNoteByPosition(NotePosition pos)
        {
            if (!groundNotes.TryGetValue(pos, out var note))
            {
                return null;
            }
            return note;
        }

        public bool RemoveNote(Note note)
        {
            if (!ChartUtility.IsFuwafuwa(note))
            {
                var pos = ChartUtility.GetPosition(note);
                if (!groundNotes.ContainsKey(pos))
                    return false;
                groundNotes.Remove(pos);
            }
            chart.notes.Remove(note);
            onNoteRemoved.Invoke(note);
            return true;
        }

        public void SetGridDivision(int div)
        {
            if (div == editor.gridDivision)
                return;
            editor.gridDivision = div;
            onGridModifed.Invoke();
        }

        public void IncreaseBarHeight(int delta)
        {
            int result = Mathf.Clamp(EditorInfo.MIN_BAR_HEIGHT, EditorInfo.MAX_BAR_HEIGHT, delta);
            if (result == editor.barHeight)
                return;
            editor.barHeight = result;
            onGridModifed.Invoke();
        }
    }
}
