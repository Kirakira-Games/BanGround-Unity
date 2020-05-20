using UnityEngine;
using UnityEngine.Events;

namespace BGEditor
{
    public class NoteEvent : UnityEvent<Note> { }

    public class ChartCore : MonoBehaviour
    {
        public static ChartCore Instance;

        public UnityEvent onGridMoved;
        public UnityEvent onTimingModified;
        public UnityEvent onGridDivisionModifed;
        public NoteEvent onNoteCreated;
        public NoteEvent onNoteModified;
        public NoteEvent onNoteRemoved;

        void Awake()
        {
            Instance = this;

            // Initialize events
            onGridMoved = new UnityEvent();
            onTimingModified = new UnityEvent();
            onGridDivisionModifed = new UnityEvent();
            onNoteCreated = new NoteEvent();
            onNoteModified = new NoteEvent();
            onNoteRemoved = new NoteEvent();
        }
    }
}
