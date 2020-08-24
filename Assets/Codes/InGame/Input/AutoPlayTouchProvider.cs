using UnityEngine;
using System.Collections.Generic;

public class AutoPlayTouchProvider : KirakiraTouchProvider
{
    private List<KirakiraTouchState> events = new List<KirakiraTouchState>();
    private LinkedList<KirakiraTouchState> delayedQueue = new LinkedList<KirakiraTouchState>();
    private Dictionary<int, int> touchIdMap = new Dictionary<int, int>();
    private Dictionary<int, KirakiraTouchState> touchStateMap = new Dictionary<int, KirakiraTouchState>();
    private LinkedList<int> touchIdPool = new LinkedList<int>();
    private HashSet<int> existsId = new HashSet<int>();
    private int head;
    private int currentId;
    private IGameStateMachine SM;

    public AutoPlayTouchProvider(IGameStateMachine SM)
    {
        this.SM = SM;
    }

    private int GetId(int id)
    {
        if (!touchIdMap.ContainsKey(id))
        {
            touchIdMap.Add(id, NextAvailableId());
        }
        return touchIdMap[id];
    }

    private int NextAvailableId()
    {
        if (touchIdPool.Count == 0)
        {
            touchIdPool.AddLast(currentId++);
        }
        int ret = touchIdPool.Last.Value;
        touchIdPool.RemoveLast();
        return ret;
    }

    private void ReleaseId(int id)
    {
        touchIdPool.AddLast(id);
        touchStateMap.Remove(id);
    }

    private void AddTapStart(GameNoteData note)
    {
        var pos = note.pos;
        var screenPos = NoteController.mainCamera.WorldToScreenPoint(pos);
        var touchS = new KirakiraTouchState
        {
            time = note.time - 10,
            pos = pos,
            screenPos = screenPos,
            phase = KirakiraTouchPhase.Began,
            touchId = currentId
        };
        events.Add(touchS);
    }

    private void AddTapMove(GameNoteData note, int dt = 0)
    {
        var pos = note.pos;
        var screenPos = NoteController.mainCamera.WorldToScreenPoint(pos);
        var touchS = new KirakiraTouchState
        {
            time = note.time + dt,
            pos = pos,
            screenPos = screenPos,
            phase = KirakiraTouchPhase.Ongoing,
            touchId = currentId
        };
        events.Add(touchS);
    }

    private void AddTapEnd(GameNoteData note, bool needMove)
    {
        var pos = note.pos;
        if (NoteUtility.IsFlick(note.type))
        {
            if (needMove)
            {
                var last = events[events.Count - 1];
                AddTapMove(note, -Mathf.Min(note.time - last.time - 1, 20));
            }
            pos.y += 1f;
        }
        var screenPos = NoteController.mainCamera.WorldToScreenPoint(pos);
        var touchT = new KirakiraTouchState
        {
            time = note.time + 1,
            pos = pos,
            screenPos = screenPos,
            phase = KirakiraTouchPhase.Ended,
            touchId = currentId
        };
        events.Add(touchT);
        currentId++;
    }

    public void Init(GameNoteData[] notes)
    {
        currentId = 0;
        head = 0;
        touchIdMap.Clear();
        touchIdPool.Clear();
        touchStateMap.Clear();
        events.Clear();
        delayedQueue.Clear();
        foreach (var note in notes)
        {
            if (NoteUtility.IsSlide(note.type))
            {
                AddTapStart(note.seg[0]);
                for (int i = 1; i < note.seg.Count - 1; i++)
                {
                    AddTapMove(note.seg[i]);
                }
                AddTapEnd(note.seg[note.seg.Count - 1], true);
            }
            else
            {
                AddTapStart(note);
                AddTapEnd(note, false);
            }
        }
        events.Sort((lhs, rhs) => lhs.time.CompareTo(rhs.time));
        currentId = 0;

        // Sanity check - Debug only
#if UNITY_EDITOR
        HashSet<int> hasStarted = new HashSet<int>();
        HashSet<int> hasEnded = new HashSet<int>();
        foreach (var e in events)
        {
            if (e.phase == KirakiraTouchPhase.Began)
            {
                Debug.Assert(!hasStarted.Contains(e.touchId));
                Debug.Assert(!hasEnded.Contains(e.touchId));
                hasStarted.Add(e.touchId);
            }
            else if (e.phase == KirakiraTouchPhase.Ended)
            {
                Debug.Assert(hasStarted.Contains(e.touchId));
                Debug.Assert(!hasEnded.Contains(e.touchId));
                hasEnded.Add(e.touchId);
            }
            else
            {
                Debug.Assert(e.phase == KirakiraTouchPhase.Ongoing);
                Debug.Assert(hasStarted.Contains(e.touchId));
                Debug.Assert(!hasEnded.Contains(e.touchId));
            }
        }
        foreach (var e in hasStarted)
        {
            Debug.Assert(hasEnded.Contains(e));
        }
#endif
    }

    public KirakiraTouchState[][] GetTouches()
    {
        existsId.Clear();
        var ret = new List<KirakiraTouchState>();
        LinkedListNode<KirakiraTouchState> next;

        for (var i = delayedQueue.First; i != null; i = next)
        {
            var state = i.Value;
            next = i.Next;
            if (existsId.Contains(state.touchId))
            {
                continue;
            }
            existsId.Add(state.touchId);
            ret.Add(state);
            delayedQueue.Remove(i);
        }

        while (head < events.Count && events[head].time <= NoteController.audioTime)
        {
            if (SM.isRewinding) break;
            var cur = events[head];
            if (cur.phase == KirakiraTouchPhase.Began)
            {
                Debug.Assert(!touchIdMap.ContainsKey(cur.touchId));
            }
            else
            {
                Debug.Assert(touchIdMap.ContainsKey(cur.touchId));
            }
            cur.touchId = GetId(cur.touchId);
            if (cur.phase == KirakiraTouchPhase.Began)
            {
                Debug.Assert(!touchStateMap.ContainsKey(cur.touchId));
                Debug.Assert(!existsId.Contains(cur.touchId));
            }
            if (existsId.Contains(cur.touchId))
            {
                Debug.Assert(cur.phase != KirakiraTouchPhase.Began);
                delayedQueue.AddLast(cur);
            }
            else
            {
                existsId.Add(cur.touchId);
                ret.Add(cur);
            }
            head++;
        }

        // Release ended touch IDs
        foreach (var e in ret)
        {
            if (e.phase == KirakiraTouchPhase.Ended)
            {
                //Debug.Log("Release: " + e.touchId);
                ReleaseId(e.touchId);
            }
            else if (e.phase == KirakiraTouchPhase.Began)
            {
                //Debug.Log("Add: " + e.touchId);
                Debug.Assert(!touchStateMap.ContainsKey(e.touchId));
                touchStateMap.Add(e.touchId, e);
            }
            else
            {
                //Debug.Log("Update: " + e.touchId);
                Debug.Assert(touchStateMap.ContainsKey(e.touchId));
                touchStateMap[e.touchId] = e;
            }
        }

        foreach (var entry in touchStateMap)
        {
            var touch = entry.Value;
            if (existsId.Contains(touch.touchId))
            {
                continue;
            }
            ret.Add(touch);
            touch.phase = KirakiraTouchPhase.Ongoing;
        }

        foreach (var e in ret)
        {
            e.time = NoteController.audioTime;
            e.realtime = Time.realtimeSinceStartup;
        }

        return new KirakiraTouchState[][] { ret.ToArray() };
    }
}
