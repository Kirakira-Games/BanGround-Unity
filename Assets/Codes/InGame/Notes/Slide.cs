﻿using System;
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

    public int GetTouchId()
    {
        return touchId;
    }

    public void OnNoteDestroy()
    {
        UnbindTouch();
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
        noteHead.isTilt = Vector3.Distance(notes[1].judgePos, noteHead.judgePos) >= NoteUtility.EPS;
        noteHead.tapEffect.gameObject.SetActive(false);
        SlideNoteBase lastNote = notes[notes.Count - 1];
        lastNote.isTilt = Vector3.Distance(notes[notes.Count - 2].judgePos, lastNote.judgePos) >= NoteUtility.EPS;
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
            mesh.afterNoteTrans = lastNote.transform;
        }
        else
        {
            bool enableBody;
            var next = notes[displayHead];
            var prev = notes[displayHead - 1];
            mesh.afterNoteTrans = next.transform;

            var intersect = FindSlideIntersection();
            if (!intersect.HasValue)
            {
                float percentage = (float)(NoteController.audioTime - prev.time) / (next.time - prev.time);
                percentage = Mathf.Max(0, percentage);
                noteHead.transform.position = Vector3.LerpUnclamped(prev.judgePos, next.judgePos, percentage);
                enableBody = displayHead == 1 || !prev.gameObject.activeSelf;
            }
            else
            {
                noteHead.transform.position = intersect.Value;
                enableBody = false;
            }
            mesh.meshRenderer.enabled = enableBody;
        }
        noteHead.gameObject.SetActive(isJudging || LiveSetting.autoPlayEnabled);
        noteHead.tapEffect.OnUpdate();
    }

    public void OnSlideUpdate()
    {
    	var _notes = GetComponentsInChildren<SlideNoteBase>();

        // Update ticks
        foreach (var note in _notes)
        {
            note.OnNoteUpdate();
        }

        // Update head
        UpdateHead();
        UpdateDisplayHead();
        if (judgeHead >= notes.Count)
        {
            NotePool.instance.DestroySlide(gameObject);
            return;
        }

        // Update position of noteHead
        if (noteHead.judgeResult != JudgeResult.None)
        {
            UpdateNoteHead();
        }

        // Update mesh
        foreach (var note in _notes)
        {
            note.slideMesh?.OnUpdate();
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

    private void BindTouch(KirakiraTouch touch)
    {
        if (LiveSetting.autoPlayEnabled || isJudging || touch == null) return;
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
        if (note.isTracingOrJudged)
        {
            note.Trace(touch, result);
        }
        else if (note.TryJudge(touch) != JudgeResult.None)
        {
            note.Judge(touch, result);
        }
        if (touch.current.phase == KirakiraTouchPhase.Ended)
        {
            UpdateHead();
            if (judgeHead < notes.Count)
            {
                NoteController.instance.Judge(notes[judgeHead], JudgeResult.Miss, touch);
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
