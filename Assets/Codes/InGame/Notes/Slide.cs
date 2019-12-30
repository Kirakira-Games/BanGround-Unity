using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    private ArrayList notes;
    private int noteHead;
    private int touchId;

    public int GetTouchId()
    {
        return touchId;
    }

    public void InitSlide()
    {
        notes = new ArrayList();
        touchId = -1;
        noteHead = 0;
    }

    public void OnSlideUpdate()
    {
        NoteBase[] childNotes = GetComponentsInChildren<NoteBase>();
        foreach (NoteBase note in childNotes)
        {
            note.OnNoteUpdate();
        }
        foreach (NoteBase note in childNotes)
        {
            note.GetComponentInChildren<NoteMesh>()?.OnUpdate();
        }
    }

    public void AddNote(NoteBase note)
    {
        note.transform.SetParent(transform);
        if (notes.Count > 0)
        {
            NoteMesh.Create((notes[notes.Count - 1] as NoteBase).transform, note.transform);
        }
        notes.Add(note);
    }

    public void Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        if (touchId != -1) return;
    }
}
