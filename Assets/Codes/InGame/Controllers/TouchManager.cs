using UnityEngine;
using System;
using System.Collections.Generic;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public interface KirakiraTracer
{
    /// <summary>
    /// Returns current position of the tracer.
    /// </summary>
    Vector2 GetPosition();

    /// <summary>
    /// Try to trace the touch, but do not perform any side effects.
    /// </summary>
    JudgeResult TryTrace(KirakiraTouch touch);

    /// <summary>
    /// Actually tracing the touch. Guaranteed to produce some result.
    /// </summary>
    void Trace(KirakiraTouch touch, JudgeResult result);

    /// <summary>
    /// Assign / Reasssign a touch to this tracer.
    /// </summary>
    void Assign(KirakiraTouch touch);
}

public enum KirakiraTouchPhase
{
    NONE, BEGAN, ONGOING, ENDED
}

public class KirakiraTouchState
{
    /// <summary>
    /// Time of receiving this touch.
    /// </summary>
    public int time;

    /// <summary>
    /// Unique ID of this touch.
    /// </summary>
    public int touchId;

    /// <summary>
    /// Touch position on screen.
    /// </summary>
    public Vector2 screenPos;

    /// <summary>
    /// Touch position on judge plane.
    /// </summary>
    public Vector2 pos;

    /// <summary>
    /// Phase of the touch.
    /// </summary>
    public KirakiraTouchPhase phase;
}

public class KirakiraTouch
{
    /// <summary>
    /// FingerId of this touch.
    /// </summary>
    public int touchId;

    /// <summary>
    /// Whether this is a valid entry.
    /// </summary>
    public bool isValid => touchId != -1;

    /// <summary>
    /// The GameObject owning this touch.
    /// </summary>
    public KirakiraTracer owner;

    /// <summary>
    /// Duration since this touch moved flick distance.
    /// </summary>
    public int timeSinceFlick { get; private set; }

    /// <summary>
    /// Duration since this touch starts.
    /// </summary>
    public int duration => current.time - start.time;

    /// <summary>
    /// Whether current position of the touch is flick distance away from its initial position.
    /// </summary>
    public bool hasMovedFlickDist => Vector3.Distance(current.screenPos, start.screenPos) >= NoteUtility.FLICK_JUDGE_DIST;

    /// <summary>
    /// Current touch state.
    /// </summary>
    public KirakiraTouchState current => timeline.LastV.Value;

    /// <summary>
    /// Initial touch state.
    /// </summary>
    public KirakiraTouchState start { get; private set; }

    /// <summary>
    /// Set of exchangable touch IDs.
    /// </summary>
    public HashSet<int> exchangable;

    private PriorityQueue<int, KirakiraTouchState> timeline;
    public static int INVALID_DURATION => NoteUtility.SLIDE_TICK_JUDGE_RANGE << 1;
    public static Vector3 INVALID_POSITION => new Vector3(0, 0, -1e3f);

    /// <summary>
    /// The distance between two screen points, converted to cm.
    /// </summary>
    public static float DistanceInCm(Vector2 p, Vector2 q)
    {
        return Vector2.Distance(p, q) * 2.54F / Screen.dpi;
    }

    public static KirakiraTouchPhase Kirakira(TouchPhase phase)
    {
        switch (phase)
        {
            case TouchPhase.Began:
                return KirakiraTouchPhase.BEGAN;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                return KirakiraTouchPhase.ONGOING;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                return KirakiraTouchPhase.ENDED;
            default:
                return KirakiraTouchPhase.NONE;
        }
    }

    public KirakiraTouch()
    {
        timeline = new PriorityQueue<int, KirakiraTouchState>();
        exchangable = new HashSet<int>();
        Reset();
    }

    public void Reset()
    {
        timeline.Clear();
        exchangable.Clear();
        touchId = -1;
        start = null;
        owner = null;
        timeSinceFlick = INVALID_DURATION;
    }

    public void OnUpdate(KirakiraTouchState state)
    {
        // Add current touch
        if (start == null)
        {
            start = state;
        }
        timeline.Push(state.time, state);

        // Remove unnecessary states
        while (timeline.FirstV != null && current.time - timeline.FirstV.Value.time > NoteUtility.SLIDE_TICK_JUDGE_RANGE)
        {
            timeline.RemoveFirst();
        }

        // Update time
        timeSinceFlick = INVALID_DURATION;
        for (var i = timeline.LastV; i != null; i = i.Previous)
        {
            if (DistanceInCm(i.Value.screenPos, current.screenPos) <= NoteUtility.FLICK_JUDGE_DIST)
            {
                timeSinceFlick = Mathf.RoundToInt(state.time * 1000) - i.Value.time;
                break;
            }
        }
    }
}

public class TouchManager : MonoBehaviour
{
    public static TouchManager instance;

    private Dictionary<int, KirakiraTouch> touchTable;
    private Dictionary<(KirakiraTracer, int), JudgeResult> traceCache;

    public static int EvalResult(JudgeResult result)
    {
        switch (result)
        {
            case JudgeResult.Miss: return -1;
            case JudgeResult.Bad: return 1;
            case JudgeResult.Good: return 2;
            case JudgeResult.Great: return 3;
            case JudgeResult.Perfect: return 4;
            default: return 0;
        }
    }

    // For debugging purpose only, simulate touch event from mouse event
    public static KirakiraTouchState[] SimulateMouseTouch(KirakiraTouchPhase phase)
    {
        var ray = NoteController.mainCamera.ScreenPointToRay(Input.mousePosition);
        var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;
        KirakiraTouchState touch = new KirakiraTouchState
        {
            touchId = NoteUtility.MOUSE_TOUCH_ID,
            screenPos = Input.mousePosition,
            pos = pos,
            time = AudioTimelineSync.instance.GetTimeInMs(),
            phase = phase
        };
        return new KirakiraTouchState[] { touch };
    }

    public static KirakiraTouchState[] GetTouches()
    {
        var touches = Touch.activeTouches;
        KirakiraTouchState[] ret = new KirakiraTouchState[touches.Count];
        for (int i = 0; i < touches.Count; i++)
        {
            var touch = touches[i];
            var ray = NoteController.mainCamera.ScreenPointToRay(touch.screenPosition);
            var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

            ret[i] = new KirakiraTouchState
            {
                touchId = touch.touchId,
                time = Mathf.RoundToInt(AudioTimelineSync.RealTimeToBGMTime((float)touch.time) * 1000),
                screenPos = touch.screenPosition,
                pos = pos,
                phase = KirakiraTouch.Kirakira(touch.phase)
            };
        }

        // Simulate touches with mouse
        if (touches.Count == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ret = SimulateMouseTouch(KirakiraTouchPhase.BEGAN);
            }
            else if (Input.GetMouseButton(0))
            {
                ret = SimulateMouseTouch(KirakiraTouchPhase.ONGOING);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ret = SimulateMouseTouch(KirakiraTouchPhase.ENDED);
            }
        }
        return ret;
    }

    public KirakiraTouch GetTouchById(int id)
    {
        if (!touchTable.ContainsKey(id))
        {
            var ret = new KirakiraTouch();
            ret.touchId = id;
            touchTable.Add(id, ret);
        }
        return touchTable[id];
    }

    public void RegisterTouch(int id, KirakiraTracer obj)
    {
        var touch = GetTouchById(id);
        Debug.Assert(touch.owner == null);
        touch.owner = obj;
        obj.Assign(touch);
    }

    public void UnregisterTouch(int id, KirakiraTracer obj)
    {
        var touch = GetTouchById(id);
        if (ReferenceEquals(touch.owner, obj))
        {
            touch.owner = null;
            obj.Assign(null);
        }
        else
        {
            Debug.LogWarning("Invalid removal from touchTable: " + id);
        }
    }

    public bool IsTracing(int touchId)
    {
        var touch = GetTouchById(touchId);
        return touch.isValid && touch.owner != null;
    }

    private void Awake()
    {
        instance = this;
        touchTable = new Dictionary<int, KirakiraTouch>();
        traceCache = new Dictionary<(KirakiraTracer, int), JudgeResult>();
    }

    public static bool TouchesNote(KirakiraTouchState touch, Vector2 note)
    {
        return Vector2.Distance(touch.pos, note) <= NoteUtility.FUWAFUWA_RADIUS;
    }

    public static bool TouchesNote(KirakiraTouchState touch, int lane)
    {
        var judgePos = NoteUtility.GetJudgePos(lane);
        return Mathf.Abs(touch.pos.x - judgePos.x) <= NoteUtility.LANE_JUDGE_WIDTH &&
                Mathf.Abs(touch.pos.y - judgePos.y) <= NoteUtility.LANE_JUDGE_HEIGHT;
    }

    public static bool TouchesNote(KirakiraTouchState touch, NoteBase note)
    {
        if (note.isFuwafuwa)
        {
            return TouchesNote(touch, note.judgePos);
        }
        else
        {
            return TouchesNote(touch, note.lane);
        }
    }

    private void ExchangeTouch(KirakiraTouch touch1, KirakiraTouch touch2)
    {
        (touch1.owner, touch2.owner) = (touch2.owner, touch1.owner);
        touch1.owner?.Assign(touch1);
        touch2.owner?.Assign(touch2);
    }

    public void OnUpdate()
    {
        var touches = GetTouches();

        // Update touches that just starts
        foreach (var touch1 in touches)
        {
            var tracer = GetTouchById(touch1.touchId);
            if (touch1.phase == KirakiraTouchPhase.BEGAN)
            {
                tracer.Reset();
                tracer.touchId = touch1.touchId;
            }
            tracer.OnUpdate(touch1);
        }

        // Compute exchangable touches
        foreach (var touch1 in touches)
        {
            var tracer1 = GetTouchById(touch1.touchId);
            if (!tracer1.isValid)
            {
                continue;
            }
            var owner1 = touchTable[touch1.touchId].owner;
            // If exchangable, the touch must lie in both judge areas.
            if (owner1 == null || !TouchesNote(touch1, owner1.GetPosition()))
            {
                continue;
            }
            foreach (var entry in touchTable)
            {
                if (!entry.Value.isValid)
                {
                    continue;
                }
                var tracer2 = entry.Value;
                var owner2 = tracer2.owner;
                if (owner2 == null || !TouchesNote(touch1, owner2.GetPosition()))
                {
                    continue;
                }
                tracer2.exchangable.Add(touch1.touchId);
            }
        }

        // Try exchanging touches
        traceCache.Clear();
        foreach (var entry in touchTable)
        {
            var tracer = entry.Value;
            var owner = tracer.owner;
            if (!tracer.isValid || owner == null)
            {
                continue;
            }
            foreach (var i in tracer.exchangable)
            {
                var tracer2 = GetTouchById(i);
                Debug.Assert(tracer2.isValid);
                traceCache.Add((owner, i), owner.TryTrace(tracer2));
            }
        }
        bool hasExchanged;
        do
        {
            hasExchanged = false;
            foreach (var entry in touchTable)
            {
                var tracer = entry.Value;
                var owner = tracer.owner;
                if (!tracer.isValid || owner == null)
                {
                    continue;
                }
                foreach (var i in tracer.exchangable)
                {
                    var tracer2 = GetTouchById(i);
                    Debug.Assert(tracer2.isValid);
                    var owner2 = tracer2.owner;
                    int prevVal = EvalResult(traceCache[(owner, entry.Key)]) +
                        (owner2 == null ? 0 : EvalResult(traceCache[(owner2, i)]));
                    int exchangeVal = EvalResult(traceCache[(owner, i)]) +
                        (owner2 == null ? 0 : EvalResult(traceCache[(owner2, entry.Key)]));
                    if (exchangeVal > prevVal)
                    {
                        hasExchanged = true;
                        ExchangeTouch(tracer, tracer2);
                        break;
                    }
                }
            }
        } while (hasExchanged);

        // Actually trace touches
        foreach (var entry in touchTable)
        {
            var tracer = entry.Value;
            if (!tracer.isValid || tracer.owner == null)
            {
                continue;
            }
            tracer.owner.Trace(tracer, traceCache[(tracer.owner, entry.Key)]);
        }

        // Process other touches
        foreach (var touch in touches)
        {
            if (IsTracing(touch.touchId))
            {
                continue;
            }
            NoteController.instance.UpdateTouch(GetTouchById(touch.touchId));
        }

        // Remove ended touches
        foreach (var touch1 in touches)
        {
            var tracer = GetTouchById(touch1.touchId);
            if (touch1.phase == KirakiraTouchPhase.ENDED)
            {
                foreach (var entry in touchTable)
                {
                    entry.Value.exchangable.Remove(touch1.touchId);
                }
                tracer.touchId = -1;
            }
        }
    }
}
