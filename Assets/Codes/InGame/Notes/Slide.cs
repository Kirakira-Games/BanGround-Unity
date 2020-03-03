using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    private List<SlideNoteBase> notes;
    private int judgeHead;
    private int displayHead;
    private int touchId;
    private SlideStart noteHead;

    public int GetTouchId()
    {
        return touchId;
    }

    public void OnNoteDestroy()
    {
        if (touchId != -1)
        {
            NoteController.controller.UnregisterTouch(touchId, gameObject);
            touchId = -1;
        }
        foreach (var note in notes)
        {
            NotePool.instance.DestroyNote(note.gameObject);
        }
    }

    public void InitSlide()
    {
        notes = new List<SlideNoteBase>();
        touchId = -1;
        judgeHead = 0;
        displayHead = 1;
        noteHead = null;
    }

    public void FinalizeSlide()
    {
        noteHead.IsTilt = notes[1].lane != noteHead.lane;
        noteHead.GetComponentInChildren<TapEffect>(true).gameObject.SetActive(false);
        SlideNoteBase lastNote = notes[notes.Count - 1];
        lastNote.IsTilt = notes[notes.Count - 2].lane != lastNote.lane;
        foreach (var note in notes)
        {
            note.InitSlideNote();
        }
    }

    private void UpdateHead()
    {
        while (judgeHead < notes.Count && (notes[judgeHead].isDestroyed ||
            notes[judgeHead].judgeResult != JudgeResult.None))
        {
            judgeHead++;
        }
    }

    private void UpdateDisplayHead(int audioTime)
    {
        while (displayHead < notes.Count &&
            (audioTime >= notes[displayHead].time ||
             notes[displayHead].judgeResult != JudgeResult.None))
        {
            displayHead++;
        }
    }

    private void UpdateNoteHead(int audioTime)
    {
        if (displayHead >= notes.Count)
        {
            var lastNote = notes[notes.Count - 1];
            noteHead.transform.position = NoteUtility.GetJudgePos(lastNote.lane);
            noteHead.GetComponentInChildren<SlideMesh>().afterNoteTrans = lastNote.transform;
        }
        else
        {
            var next = notes[displayHead];
            var prev = notes[displayHead - 1];
            prev.UpdatePosition(audioTime);
            Vector3 prevPos = prev.transform.position;
            Vector3 nextPos = next.transform.position;
            float percentage = Mathf.Abs(nextPos.z - prevPos.z) <= NoteUtility.EPS ?
                (audioTime - prev.time) / (next.time - prev.time) :
                (NoteUtility.NOTE_JUDGE_POS - prevPos.z) / (nextPos.z - prevPos.z);
            percentage = Mathf.Max(0, percentage);
            noteHead.transform.position = (next.judgePos - prev.judgePos) * percentage + prev.judgePos;
            SlideMesh mesh = noteHead.slideMesh;
            mesh.afterNoteTrans = next.transform;
            mesh.meshRenderer.enabled = displayHead == 1 || !prev.gameObject.activeSelf;
        }
        noteHead.gameObject.SetActive(touchId != -1 || LiveSetting.autoPlayEnabled);
    }

    public void TraceTouch(int audioTime, Touch touch)
    {
        UpdateHead();
        if (judgeHead >= notes.Count) return;
        var note = notes[judgeHead];
        if (note.touchId == touch.fingerId)
        {
            note.TraceTouch(audioTime, touch);
        }
        else
        {
            int[] lanes = NoteController.GetLanesByTouchPosition(touch.position);
            foreach (int lane in lanes)
            {
                if (Mathf.Abs(lane - note.lane) <= 1)
                {
                    JudgeResult result = note.TryJudge(audioTime, touch);
                    if (result != JudgeResult.None)
                    {
                        note.Judge(audioTime, result, touch);
                    }
                    break;
                }
            }
        }
        if (NoteUtility.IsTouchEnd(touch))
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

    public void OnSlideUpdate(int audioTime)
    {
    	var _notes = GetComponentsInChildren<SlideNoteBase>();

        // Update ticks
        foreach (var note in _notes)
        {
            note.OnNoteUpdate(audioTime);
        }

        // Update head
        UpdateHead();
        UpdateDisplayHead(audioTime);
        if (judgeHead >= notes.Count)
        {
            NotePool.instance.DestroySlide(gameObject);
            return;
        }

        // Update position of noteHead
        if (noteHead.judgeResult != JudgeResult.None)
        {
            UpdateNoteHead(audioTime);
        }

        // Update mesh
        foreach (var note in _notes)
        {
            note.slideMesh?.OnUpdate();
            note.GetComponentInChildren<TapEffect>()?.OnUpdate();
        }
    }

    public void AddNote(NoteBase note)
    {
        note.transform.SetParent(transform);
        if (notes.Count > 0)
        {
            SlideMesh.Create(notes[notes.Count-1].GetComponentInChildren<SlideMesh>(), note.transform);
        }
        else
        {
            noteHead = note as SlideStart;
        }
        notes.Add((SlideNoteBase)note);
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
        if (judgeHead >= notes.Count || !ReferenceEquals(note.GetComponent<SlideNoteBase>(), notes[judgeHead]))
        {
            if (notes.IndexOf(note.GetComponent<SlideNoteBase>()) < judgeHead)
            {
                note.SetActive(false);
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
        if (judgeHead == 0)
        {
            noteHead.GetComponentInChildren<TapEffect>(true).gameObject.SetActive(true);
        }
        else
        {
            note.SetActive(false);
        }
        judgeHead++;
        return true;
    }
}
