using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    private ArrayList notes;
    private int judgeHead;
    private int displayHead;
    private int touchId;
    private SlideStart noteHead;

    public int GetTouchId()
    {
        return touchId;
    }

    private void OnDestroy()
    {
        if (touchId != -1)
        {
            NoteController.controller.UnregisterTouch(touchId);
            touchId = -1;
        }
    }

    public void InitSlide()
    {
        notes = new ArrayList();
        touchId = -1;
        judgeHead = 0;
        displayHead = 1;
    }

    private void UpdateHead()
    {
        while (judgeHead < notes.Count && (notes[judgeHead] == null ||
            (notes[judgeHead] as NoteBase).judgeTime != -1))
        {
            judgeHead++;
        }
    }

    private void UpdateDisplayHead()
    {
        int audioTime = (int)(Time.time * 1000);
        while (displayHead < notes.Count &&
            (audioTime >= (notes[displayHead] as NoteBase).time))
        {
            displayHead++;
        }
    }

    public void TraceTouch(int audioTime, Touch touch)
    {
        UpdateHead();
        if (judgeHead >= notes.Count) return;
        NoteBase note = notes[judgeHead] as NoteBase;
        if (note.touchId == touch.fingerId)
        {
            note.TraceTouch(audioTime, touch);
        }
        else
        {
            int lane = NoteController.GetLaneByTouchPosition(touch.position);
            if (lane == -1) return;
            JudgeResult result = note.TryJudge(audioTime, touch);
            if (result != JudgeResult.None)
            {
                note.Judge(audioTime, result, touch);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                NoteController.controller.Judge(note.gameObject, JudgeResult.Miss, touch);
                judgeHead++;
            }
        }
    }

    public void OnSlideUpdate()
    {
        // Update ticks
        NoteBase[] childNotes = GetComponentsInChildren<NoteBase>();
        foreach (NoteBase note in childNotes)
        {
            note.OnNoteUpdate();
        }
        // Update head
        UpdateHead();
        UpdateDisplayHead();
        // Update position of noteHead
        int audioTime = (int)(Time.time * 1000);
        if (noteHead.judgeTime != -1)
        {
            if (displayHead >= notes.Count)
            {
                NoteBase lastNote = notes[notes.Count - 1] as NoteBase;
                noteHead.transform.position = NoteUtility.GetInitPos(lastNote.lane);
                noteHead.GetComponentInChildren<NoteMesh>().afterNoteTrans = lastNote.transform;
            }
            else
            {
                NoteBase next = notes[displayHead] as NoteBase;
                NoteBase prev = notes[displayHead - 1] as NoteBase;
                float percentage = (float)(audioTime - prev.time) / (next.time - prev.time);
                percentage = Mathf.Max(0, percentage);
                Vector3 prevPos = prev.initPos;
                Vector3 nextPos = next.initPos;
                noteHead.transform.position = (nextPos - prevPos) * percentage + prevPos;
                noteHead.GetComponentInChildren<NoteMesh>().afterNoteTrans = next.transform;
            }
        }
        // Update mesh
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
        else
        {
            noteHead = note as SlideStart;
        }
        notes.Add(note);
    }

    public bool Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        // Must judge head
        if (judgeHead >= notes.Count || !ReferenceEquals(note.GetComponent<NoteBase>(), notes[judgeHead] as NoteBase))
        {
            if (notes.IndexOf(note.GetComponent<NoteBase>()) < judgeHead)
            {
                if (touch.HasValue)
                {
                    touchId = touch.Value.fingerId;
                }
                Destroy(note);
            }
            return false;
        }
        if (touch.HasValue)
        {
            touchId = touch.Value.fingerId;
        }
        NoteController.controller.Judge(note, result, touch);
        if (judgeHead > 0)
        {
            Destroy(note);
        }
        judgeHead++;
        return true;
    }
}
