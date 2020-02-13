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
            NoteController.controller.UnregisterTouch(touchId, gameObject);
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

    private void UpdateDisplayHead(int audioTime)
    {
        while (displayHead < notes.Count &&
            (audioTime >= (notes[displayHead] as NoteBase).time ||
             (notes[displayHead] as NoteBase).judgeResult != JudgeResult.None))
        {
            displayHead++;
        }
    }

    private void UpdateNoteHead(int audioTime)
    {
        if (displayHead >= notes.Count)
        {
            NoteBase lastNote = notes[notes.Count - 1] as NoteBase;
            noteHead.transform.position = NoteUtility.GetJudgePos(lastNote.lane);
            noteHead.GetComponentInChildren<SlideMesh>().afterNoteTrans = lastNote.transform;
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
            SlideMesh mesh = noteHead.GetComponentInChildren<SlideMesh>();
            mesh.afterNoteTrans = next.transform;
            mesh.GetComponent<MeshRenderer>().enabled = displayHead == 1 || !prev.gameObject.activeSelf;
        }
        noteHead.gameObject.SetActive(touchId != -1 || LiveSetting.autoPlayEnabled);
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
    	var _notes = GetComponentsInChildren<NoteBase>();

        // Update ticks
        foreach (NoteBase note in _notes)
        {
            note.OnNoteUpdate(audioTime);
        }

        // Update head
        UpdateHead();
        UpdateDisplayHead(audioTime);
        if (judgeHead >= notes.Count)
        {
            Destroy(gameObject);
            return;
        }

        // Update position of noteHead
        if (noteHead.judgeResult != JudgeResult.None)
        {
            UpdateNoteHead(audioTime);
        }

        // Update mesh
        foreach (NoteBase note in _notes)
        {
            note.GetComponentInChildren<SlideMesh>()?.OnUpdate();
            note.GetComponentInChildren<TapEffect>()?.OnUpdate();
        }
    }

    public void AddNote(NoteBase note)
    {
        note.transform.SetParent(transform);
        if (notes.Count > 0)
        {
            SlideMesh.Create((notes[notes.Count - 1] as NoteBase).transform, note.transform);
        }
        else
        {
            noteHead = note as SlideStart;
        }
        notes.Add(note);
        (note as SlideNoteBase).InitSlideNote();
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
        if (judgeHead == 0)
        {
            var obj = Instantiate(Resources.Load("Effects/effect_TapKeep"), noteHead.transform) as GameObject;
            obj.AddComponent<TapEffect>();
        }
        else
        {
            note.gameObject.SetActive(false);
        }
        judgeHead++;
        return true;
    }
}
