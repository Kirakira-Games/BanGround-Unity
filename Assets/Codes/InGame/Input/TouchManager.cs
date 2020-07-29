using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Codes.InGame.Input;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Linq;

public interface KirakiraTouchProvider
{
    KirakiraTouchState[][] GetTouches();
}

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
    None, Began, Ongoing, Ended
}

public class KirakiraTouchState
{
    /// <summary>
    /// Time of receiving this touch. (Judge time)
    /// </summary>
    public int time;

    /// <summary>
    /// Time that syncs with RealtimeSinceStartup
    /// </summary>
    public float realtime;

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

    public override string ToString()
    {
        return string.Format("[{0}] At {1} / {2}, Phase = {3}", touchId, pos, screenPos, Enum.GetName(typeof(KirakiraTouchPhase), phase));
    }
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
    public bool hasMovedFlickDist => TraveledFlickDistance(current.screenPos, start.screenPos);

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

    private PriorityQueue<float, KirakiraTouchState> timeline;
    public static int INVALID_DURATION => NoteUtility.SLIDE_TICK_JUDGE_RANGE << 1;
    public static Vector3 INVALID_POSITION => new Vector3(0, 0, -1e3f);
    public static float dpi;
    public static float flickDistPixels;

    /// <summary>
    /// Test if distance between two screen points triggers flick.
    /// </summary>
    public static bool TraveledFlickDistance(Vector2 p, Vector2 q)
    {
        return Vector2.Distance(p, q) >= flickDistPixels;
    }

    public KirakiraTouch()
    {
        timeline = new PriorityQueue<float, KirakiraTouchState>();
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

    public static int RealtimeToBGMMs(float t1, float t2)
    {
        return Mathf.RoundToInt(AudioTimelineSync.RealTimeToBGMTime(t2 - t1) * 1000);
    }

    public void OnUpdate(KirakiraTouchState state)
    {
        // Add current touch
        if (start == null)
        {
            start = state;
            touchId = state.touchId;
        }
        timeline.Push(state.realtime, state);

        // Remove unnecessary states
        while (timeline.FirstV != null &&
            RealtimeToBGMMs(timeline.FirstV.Value.realtime, current.realtime) > NoteUtility.SLIDE_TICK_JUDGE_RANGE)
        {
            timeline.RemoveFirst();
        }

        // Update time
        timeSinceFlick = INVALID_DURATION;
        for (var i = timeline.LastV; i != null; i = i.Previous)
        {
            if (TraveledFlickDistance(i.Value.screenPos, current.screenPos))
            {
                timeSinceFlick = RealtimeToBGMMs(i.Value.realtime, state.realtime);
                break;
            }
        }
    }

    public override string ToString()
    {
        string ret = "";
        for (var i = timeline.FirstV; i != null; i = i.Next)
        {
            ret += i.Value + "\n";
        }
        ret += $"dist = {Vector2.Distance(start.screenPos, current.screenPos)}, thres = {flickDistPixels}";
        return ret;
    }
}

public class TouchManager : MonoBehaviour
{
    public static TouchManager instance;
    public static KirakiraTouchProvider provider;

    private Dictionary<int, KirakiraTouch> touchTable;
    private Dictionary<(KirakiraTracer, int), JudgeResult> traceCache;
    private HashSet<KirakiraTouch> exchanged;
    private DemoRecorder recorder = null;


    public static KVar g_demoRecord = new KVar("g_demoRecord", "1", KVarFlags.Archive, "Enables demo recording.");

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

    public KirakiraTouch GetTouchById(int id)
    {
        Debug.Assert(id != -1);
        if (!touchTable.ContainsKey(id))
        {
            var ret = new KirakiraTouch();
            touchTable.Add(id, ret);
        }
        return touchTable[id];
    }

    public JudgeResult TryTrace(KirakiraTracer owner, int touchId)
    {
        return TryTrace(owner, GetTouchById(touchId));
    }

    public JudgeResult TryTrace(KirakiraTracer owner, KirakiraTouch touch)
    {
        Debug.Assert(touch.isValid);
        var pair = (owner, touch.touchId);
        if (!traceCache.ContainsKey(pair))
        {
            traceCache.Add(pair, owner.TryTrace(touch));
        }
        return traceCache[pair];
    }


    public void RegisterTouch(int id, KirakiraTracer obj)
    {
        var touch = GetTouchById(id);
        Debug.Assert(touch.isValid);
        Debug.Assert(touch.owner == null);
        touch.owner = obj;
        obj.Assign(touch);
    }

    public void UnregisterTouch(int id, KirakiraTracer obj)
    {
        var touch = GetTouchById(id);
        Debug.Assert(touch.isValid);
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

    private static float GetDPI()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
        activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

        return (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
#else
        return Screen.dpi;
#endif
    }

    static KVarRef mod_autoplay = new KVarRef("mod_autoplay");

    private void Awake()
    {
        instance = this;
        touchTable = new Dictionary<int, KirakiraTouch>();
        traceCache = new Dictionary<(KirakiraTracer, int), JudgeResult>();
        exchanged = new HashSet<KirakiraTouch>();
        KirakiraTouch.dpi = GetDPI();
        KirakiraTouch.flickDistPixels = Mathf.Min(Screen.height / 20, NoteUtility.FLICK_JUDGE_DIST / 2.54f * KirakiraTouch.dpi);

        // Touch provider
        var demoFile = LiveSetting.DemoFile;

        if (demoFile != null)
        {
            provider = new DemoReplayTouchPrivider(demoFile);
        }
        else if (mod_autoplay)
        {
            GameObject.Find("MouseCanvas").SetActive(false);
            provider = new AutoPlayTouchProvider();
        }
        else
        {
            /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        provider = new MultiMouseTouchProvider();
            #else*/
#if UNITY_EDITOR
            GameObject.Find("MouseCanvas").SetActive(false);
            provider = new MouseTouchProvider();
#else
            GameObject.Find("MouseCanvas").SetActive(false);
            provider = new InputManagerTouchProvider();
#endif
        }

        if (!(provider is DemoReplayTouchPrivider) && g_demoRecord)
        {
            recorder = new DemoRecorder(LiveSetting.CurrentHeader.sid, (Difficulty)LiveSetting.actualDifficulty);
        }
    }

    public static float TouchDist(KirakiraTouchState touch, Vector2 note)
    {
        return Vector2.Distance(touch.pos, note);
    }

    public static bool TouchesNote(KirakiraTouchState touch, Vector2 note)
    {
        return TouchDist(touch, note) <= NoteUtility.FUWAFUWA_RADIUS;
    }

    public static bool TouchesNote(KirakiraTouchState touch, int lane)
    {
        var judgePos = NoteUtility.GetJudgePos(lane);
        return Mathf.Abs(touch.pos.x - judgePos.x) <= NoteUtility.LANE_JUDGE_WIDTH &&
                Mathf.Abs(touch.pos.y - judgePos.y) <= NoteUtility.LANE_JUDGE_HEIGHT;
    }

    public static bool TouchesNote(KirakiraTouchState touch, NoteBase note)
    {
        if (note.judgeFuwafuwa)
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
        exchanged.Add(touch1);
        exchanged.Add(touch2);
        (touch1.owner, touch2.owner) = (touch2.owner, touch1.owner);
        touch1.owner?.Assign(touch1);
        touch2.owner?.Assign(touch2);
    }

    void OnDestroy()
    {
        if (recorder != null)
        {
            recorder.Save();
        }
    }

    public void OnUpdate()
    {
        if (UIManager.Instance.SM.HasState(GameStateMachine.State.Finished)) return;

        var touchFrames = provider.GetTouches();

        foreach (var touches in touchFrames)
        {
            if (recorder != null)
            {
                if (touches.Length > 0)
                {
                    recorder.Add(touches);
                }
            }

            // Update touches that just starts
            //if (touches.Length > 0)
            //{
            //    Debug.Log("===== Time: " + NoteController.audioTime);
            //}
            foreach (var touch1 in touches)
            {
                //Debug.Log(touch1);
                GetTouchById(touch1.touchId).OnUpdate(touch1);
            }

            // Compute exchangable touches
            foreach (var touch1 in touches)
            {
                var tracer1 = GetTouchById(touch1.touchId);
                Debug.Assert(tracer1.isValid);
                var owner1 = tracer1.owner;
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
            exchanged.Clear();
            bool hasExchanged;
            do
            {
                hasExchanged = false;
                if (!hasExchanged) break; // Don't exchange
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
                        int prevVal = EvalResult(TryTrace(owner, tracer)) +
                            (owner2 == null ? 0 : EvalResult(TryTrace(owner2, tracer2)));
                        int exchangeVal = EvalResult(TryTrace(owner, tracer2)) +
                            (owner2 == null ? 0 : EvalResult(TryTrace(owner2, tracer)));
                        if (exchangeVal > prevVal)
                        {
                            hasExchanged = true;
                            Debug.Log("Exchange: " + tracer.touchId + " / " + tracer2.touchId);
                            ExchangeTouch(tracer, tracer2);
                            break;
                        }
                    }
                }
            } while (hasExchanged);
            foreach (var e in exchanged)
            {
                e.exchangable.Clear();
            }

            // Actually trace touches
            foreach (var entry in touchTable)
            {
                if (!IsTracing(entry.Key))
                {
                    continue;
                }
                var tracer = entry.Value;
                tracer.owner.Trace(tracer, TryTrace(tracer.owner, tracer));
            }

            // Process other touches
            foreach (var touch in touches)
            {
                if (IsTracing(touch.touchId))
                {
                    continue;
                }
                NoteController.Instance.UpdateTouch(GetTouchById(touch.touchId));
            }

            // Remove ended touches
            foreach (var touch1 in touches)
            {
                var tracer = GetTouchById(touch1.touchId);
                if (touch1.phase == KirakiraTouchPhase.Ended)
                {
                    foreach (var entry in touchTable)
                    {
                        entry.Value.exchangable.Remove(touch1.touchId);
                    }
                    tracer.Reset();
                }
            }
        }
    }
}
