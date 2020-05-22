using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BGEditor
{
    public class NoteEvent : UnityEvent<Note> { }

    public class ChartCore : MonoBehaviour
    {
        public static ChartCore Instance;

        public GridController grid;
        public EditNoteController notes;
        public TimingController timing;
        public Camera cam;

        public GameObject SingleNote;
        public GameObject FlickNote;
        public GameObject SlideNote;

        [HideInInspector]
        public Chart chart { get; private set; }

        [HideInInspector]
        public EditorInfo editor { get; private set; }

        [HideInInspector]
        public ObjectPool pool { get; private set; }

        [HideInInspector]
        public Dictionary<NotePosition, Note> groundNotes;

        public UnityEvent onTimingModified;
        public UnityEvent onGridMoved;
        public UnityEvent onGridModifed;
        public UnityEvent onToolSwitched;
        public NoteEvent onNoteCreated;
        public NoteEvent onNoteModified;
        public NoteEvent onNoteRemoved;

        private LinkedList<IEditorCmd> cmdList;
        private LinkedListNode<IEditorCmd> lastCmd;
        private const int MAX_UNDO_COUNT = 100;

        void Awake()
        {
            Instance = this;
            chart = new Chart();

            // Initialize events
            onTimingModified = new UnityEvent();
            onGridMoved = new UnityEvent();
            onGridModifed = new UnityEvent();
            onToolSwitched = new UnityEvent();
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

            // Initialize object pool
            pool = new ObjectPool();
        }

        private void LateUpdate()
        {
            pool.PostUpdate();
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

        public bool CreateNote(Note note)
        {
            var beat = ChartUtility.BeatToFloat(note.beat);
            if (beat > editor.numBars + NoteUtility.EPS || beat < -NoteUtility.EPS)
                return false;
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
            int result = Mathf.Clamp(delta, EditorInfo.MIN_BAR_HEIGHT, EditorInfo.MAX_BAR_HEIGHT);
            if (result == editor.barHeight)
                return;
            float beat = editor.scrollPos / editor.barHeight;
            editor.barHeight = result;
            editor.scrollPos = beat * result;
            onGridModifed.Invoke();
        }

        public void SeekGrid(float target)
        {
            editor.scrollPos = Mathf.Clamp(target, 0, editor.maxHeight);
            onGridMoved.Invoke();
        }

        public void MoveGrid(float delta)
        {
            SeekGrid(editor.scrollPos + delta);
        }

        public void SwitchTool(EditorTool tool)
        {
            if (editor.tool == tool)
                return;
            editor.tool = tool;
            onToolSwitched.Invoke();
        }
    }
}
