using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    public int tickStack;
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
            NoteController.controller.UnregisterTouch(touchId, gameObject);
            touchId = -1;
        }
        NoteController.controller.EndSlide(tickStack);
    }

    public void InitSlide(int tickStack)
    {
        notes = new ArrayList();
        touchId = -1;
        judgeHead = 0;
        displayHead = 1;
        this.tickStack = tickStack;
    }

    private void Start()
    {
        noteHead.IsTilt = (notes[1] as NoteBase).lane != noteHead.lane;
        SlideNoteBase lastNote = notes[notes.Count - 1] as SlideNoteBase;
        lastNote.IsTilt = (notes[notes.Count - 2] as NoteBase).lane != lastNote.lane;
    }

    private void UpdateHead()
    {
        while (judgeHead < notes.Count && (notes[judgeHead] == null ||
            (notes[judgeHead] as NoteBase).judgeResult != JudgeResult.None))
        {
            judgeHead++;
        }
    }

    private void UpdateDisplayHead()
    {
        int audioTime = (int)(Time.time * 1000);
        while (displayHead < notes.Count &&
            (audioTime >= (notes[displayHead] as NoteBase).time ||
             (notes[displayHead] as NoteBase).judgeResult != JudgeResult.None))
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
            if (Mathf.Abs(lane - note.lane) <= 1)
            {
                JudgeResult result = note.TryJudge(audioTime, touch);
                if (result != JudgeResult.None)
                {
                    note.Judge(audioTime, result, touch);
                }
            }
        }
        if (touch.phase == TouchPhase.Ended)
        {
            UpdateHead();
            if (judgeHead < notes.Count)
            {
                NoteController.controller.Judge(note.gameObject, JudgeResult.Miss, touch);
                judgeHead++;
            }
            UnbindTouch();
        }
    }

    public void OnSlideUpdate()
    {
        // Update ticks
        foreach (NoteBase note in GetComponentsInChildren<NoteBase>())
        {
            note.OnNoteUpdate();
        }
        // Update head
        UpdateHead();
        UpdateDisplayHead();
        if (judgeHead >= notes.Count)
        {
            Destroy(gameObject);
            return;
        }
        // Update position of noteHead
        int audioTime = (int)(Time.time * 1000);
        if (noteHead.judgeTime != -1)
        {
            if (displayHead >= notes.Count)
            {
                NoteBase lastNote = notes[notes.Count - 1] as NoteBase;
                noteHead.transform.position = NoteUtility.GetJudgePos(lastNote.lane);
                noteHead.GetComponentInChildren<NoteMesh>().afterNoteTrans = lastNote.transform;
            }
            else
            {
                NoteBase next = notes[displayHead] as NoteBase;
                NoteBase prev = notes[displayHead - 1] as NoteBase;
                float percentage = (float)(audioTime - prev.time) / (next.time - prev.time);
                percentage = Mathf.Max(0, percentage);
                Vector3 prevPos = prev.judgePos;
                Vector3 nextPos = next.judgePos;
                noteHead.transform.position = (nextPos - prevPos) * percentage + prevPos;
                NoteMesh mesh = noteHead.GetComponentInChildren<NoteMesh>();
                mesh.afterNoteTrans = next.transform;
                mesh.GetComponent<MeshRenderer>().enabled = displayHead == 1 || !prev.gameObject.activeSelf;
            }
            noteHead.gameObject.SetActive(touchId != -1 || LiveSetting.autoPlayEnabled);
        }
        // Update mesh
        foreach (NoteBase note in GetComponentsInChildren<NoteBase>())
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

    private void BindTouch(Touch? touch)
    {
        if (LiveSetting.autoPlayEnabled || !touch.HasValue) return;
        touchId = touch.Value.fingerId;
        NoteController.controller.RegisterTouch(touchId, gameObject);
    }

    private void UnbindTouch()
    {
        if (touchId == -1) return;
        NoteController.controller.UnregisterTouch(touchId, gameObject);
        touchId = -1;
    }

    public bool Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        // Must judge head
        if (judgeHead >= notes.Count || !ReferenceEquals(note.GetComponent<NoteBase>(), notes[judgeHead] as NoteBase))
        {
            if (notes.IndexOf(note.GetComponent<NoteBase>()) < judgeHead)
            {
                note.gameObject.SetActive(false);
                BindTouch(touch);
                return true;
            }
            return false;
        }
        if (result == JudgeResult.Miss)
        {
            UnbindTouch();
        }
        else
        {
            BindTouch(touch);
        }
        NoteController.controller.Judge(note, result, touch);
        if (judgeHead > 0)
        {
            note.gameObject.SetActive(false);
        }
        judgeHead++;
        return true;
    }
}
