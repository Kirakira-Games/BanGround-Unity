using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour, KirakiraTracer
{
    private List<SlideNoteBase> notes;
    private int judgeHead;
    private int displayHead;
    private int touchId;
    private SlideStart noteHead;
    public bool isJudging => touchId != -1;

    private INoteController noteController;

    public int GetTouchId()
    {
        return touchId;
    }

    public void OnNoteDestroy()
    {
        UnbindTouch();
        foreach (var note in notes)
        {
            NotePool.Instance.DestroyNote(note);
        }
    }

    public void InitSlide(INoteController noteController)
    {
        this.noteController = noteController;
        notes = new List<SlideNoteBase>();
        touchId = -1;
        judgeHead = 0;
        displayHead = 1;
        noteHead = null;
    }

    public void FinalizeSlide()
    {
        bool isTilt = false;
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].judgeFuwafuwa || (i > 0 && notes[i].lane != notes[i - 1].lane))
            {
                isTilt = true;
            }
        }
        noteHead.isTilt = isTilt;
        noteHead.tapEffect.gameObject.SetActive(false);
        notes[notes.Count - 1].isTilt = isTilt;
        // GameNoteType.None for slide body which does not have a note type
        var material = noteHead.timingGroup.GetMaterial(GameNoteType.None, notes[0].slideMesh.meshRenderer.material);
        for (int i = 0; i < notes.Count - 1; i++)
        {
            notes[i].slideMesh.ResetMesh(
                notes[i].transform,
                notes[i + 1].transform,
                notes[i].displayFuwafuwa || notes[i + 1].displayFuwafuwa,
                material
            );
        }
        foreach (var note in notes)
        {
            note.ResetSlideNote(this, material);
        }
    }

    #region display
    private void UpdateHead()
    {
        while (judgeHead < notes.Count && (notes[judgeHead].isDestroyed ||
            notes[judgeHead].judgeResult != JudgeResult.None))
        {
            judgeHead++;
        }
    }

    private void UpdateDisplayHead()
    {
        while (displayHead < notes.Count &&
            (NoteController.audioTime >= notes[displayHead].time ||
             notes[displayHead].judgeResult != JudgeResult.None))
        {
            displayHead++;
        }
    }

    private Vector3? FindSlideIntersection()
    {
        for (int i = displayHead + 1; i < notes.Count; i++)
        {
            var next = notes[i];
            var prev = notes[i - 1];
            var dir = next.transform.position - prev.transform.position;
            Ray ray = new Ray(prev.transform.position, dir);
            if (NoteUtility.JudgePlane.Raycast(ray, out float dist))
            {
                if (dist < dir.magnitude - NoteUtility.EPS)
                {
                    return ray.GetPoint(dist);
                }
            }
        }
        return null;
    }

    private void UpdateNoteHead()
    {
        SlideMesh mesh = noteHead.slideMesh;
        if (displayHead >= notes.Count)
        {
            var lastNote = notes[notes.Count - 1];
            noteHead.transform.position = lastNote.judgePos;
            mesh.transT = lastNote.transform;
        }
        else
        {
            bool enableBody;
            var next = notes[displayHead];
            var prev = notes[displayHead - 1];
            mesh.transT = next.transform;

            var intersect = FindSlideIntersection();
            if (!intersect.HasValue)
            {
                float ratio = Mathf.InverseLerp(prev.time, next.time, NoteController.audioTime);
                noteHead.transform.position = Vector3.LerpUnclamped(prev.judgePos, next.judgePos, ratio);
                enableBody = displayHead == 1 || !prev.gameObject.activeSelf;
            }
            else
            {
                noteHead.transform.position = intersect.Value;
                enableBody = false;
            }
            mesh.SetFuwafuwa(prev.displayFuwafuwa || next.displayFuwafuwa);
            mesh.meshRenderer.enabled = enableBody;
        }
        noteHead.gameObject.SetActive(isJudging);
        //noteHead.tapEffect.OnUpdate();
    }
    #endregion display

    public void OnSlideUpdate()
    {
        // Update ticks
        foreach (var note in notes)
        {
            note.OnNoteUpdate();
        }

        // Update head
        UpdateHead();
        UpdateDisplayHead();
        if (judgeHead >= notes.Count)
        {
            NotePool.Instance.DestroySlide(this);
            return;
        }

        // Update position of noteHead
        if (noteHead.judgeResult != JudgeResult.None)
        {
            UpdateNoteHead();
        }

        // Update mesh
        foreach (var note in notes)
        {
            note.slideMesh?.OnUpdate();
            note.pillar?.OnUpdate();
        }
    }

    public void AddNote(NoteBase note)
    {
        note.transform.SetParent(transform);
        if (notes.Count == 0)
        {
            noteHead = note as SlideStart;
        }
        notes.Add((SlideNoteBase)note);
    }

    private void BindTouch(KirakiraTouch touch)
    {
        if (isJudging || touch == null) return;
        if (touch.current.phase == KirakiraTouchPhase.Ended) return;
        TouchManager.instance.RegisterTouch(touch.touchId, this);
        Debug.Assert(touchId == touch.touchId);
    }

    private void UnbindTouch()
    {
        if (!isJudging) return;
        TouchManager.instance.UnregisterTouch(touchId, this);
        Debug.Assert(touchId == -1);
    }

    public int Judge(SlideNoteBase note, JudgeResult result, KirakiraTouch touch)
    {
        // Must judge head
        if (judgeHead >= notes.Count || !ReferenceEquals(note, notes[judgeHead]))
        {
            if (notes.IndexOf(note) < judgeHead)
            {
                note.gameObject.SetActive(false);
                BindTouch(touch);
                return -1; // judge miss
            }
            return 0;
        }
        if (result == JudgeResult.Miss)
        {
            UnbindTouch();
        }
        else
        {
            BindTouch(touch);
        }
        if (judgeHead == 0)
        {
            noteHead.tapEffect.gameObject.SetActive(true);
        }
        else
        {
            note.gameObject.SetActive(false);
        }
        judgeHead++;
        return 1; // judge ok
    }

    public Vector2 GetPosition()
    {
        if (noteHead.judgeResult != JudgeResult.None)
        {
            return noteHead.transform.position;
        }
        else
        {
            return noteHead.judgePos;
        }
    }

    public JudgeResult TryTrace(KirakiraTouch touch)
    {
        Debug.Assert(isJudging);
        UpdateHead();
        if (judgeHead >= notes.Count) return JudgeResult.None;
        var note = notes[judgeHead];

        if (note.isTracingOrJudged)
        {
            return note.TryTrace(touch);
        }
        else
        {
            if (TouchManager.TouchesNote(touch.current, note))
            {
                var result = note.TryJudge(touch);
                if (result != JudgeResult.None)
                {
                    return result;
                }
            }
        }
        return touch.current.phase == KirakiraTouchPhase.Ended ? JudgeResult.Miss : JudgeResult.None;
    }

    public void Trace(KirakiraTouch touch, JudgeResult result)
    {
        if (result == JudgeResult.None) return;
        var note = notes[judgeHead];
        if (note.TryJudge(touch) != JudgeResult.None)
        {
            note.Judge(touch, result);
        }
        if (note.isTracingOrJudged)
        {
            var res = note.TryTrace(touch);
            if (res != JudgeResult.None)
                note.Trace(touch, res);
        }
        if (touch.current.phase == KirakiraTouchPhase.Ended)
        {
            UpdateHead();
            if (judgeHead < notes.Count)
            {
                noteController.Judge(notes[judgeHead], JudgeResult.Miss, touch);
                judgeHead++;
            }
            UnbindTouch();
        }
    }

    public void Assign(KirakiraTouch touch)
    {
        touchId = touch == null ? -1 : touch.touchId;
    }
}
