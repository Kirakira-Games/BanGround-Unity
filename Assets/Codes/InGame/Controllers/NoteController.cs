using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using AudioProvider;

using JudgeQueue = PriorityQueue<int, NoteBase>;

public class NoteController : MonoBehaviour
{
    public static NoteController instance;
    public static Camera mainCamera;

    private JudgeQueue noteQueue;
    private JudgeQueue[] laneQueue;

    private Dictionary<int, GameObject> touchTable;
    private Dictionary<int, NoteSyncLine> syncTable;

    private GameChartData chart;
    private List<GameNoteData> notes => chart.notes;
    private int noteHead;
    private int numNotes;

    private ISoundEffect[] soundEffects;

    private FixBackground background;
    private LaneEffects laneEffects;

    private UnityAction<JudgeResult> onJudge;

    private const int WARM_UP_SECOND = 4;

    public void RegisterTouch(int id, GameObject obj)
    {
        touchTable[id] = obj;
    }

    public void UnregisterTouch(int id, GameObject obj)
    {
        if (ReferenceEquals(touchTable[id], obj))
        {
            touchTable.Remove(id);
        }
        else
        {
            Debug.LogWarning("Invalid removal from touchTable: " + id);
        }
    }

    // For debugging purpose only, simulate touch event from mouse event
    static private Touch[] SimulateMouseTouch(TouchPhase phase)
    {
        Touch touch = new Touch
        {
            position = Input.mousePosition,
            fingerId = NoteUtility.MOUSE_TOUCH_ID,
            phase = phase
        };
        return new Touch[] { touch };
    }

    public TapEffectType EmitEffect(Vector3 position, JudgeResult result, GameNoteType type)
    {
        if (result == JudgeResult.Miss) return TapEffectType.None;

        TapEffectType se = TapEffectType.Click;

        if (NoteUtility.IsFlick(type))
        {
            if (result <= JudgeResult.Good)
                se = TapEffectType.Flick;
        }
        else if (result == JudgeResult.Perfect)
            se = TapEffectType.Perfect;
        else if (result == JudgeResult.Great)
            se = TapEffectType.Great;
        else if (result == JudgeResult.Good)
            se = TapEffectType.Good;

        NotePool.instance.PlayTapEffect(se, position);

        return se;
    }

    // Judge a note as result
    public void Judge(NoteBase notebase, JudgeResult result, Touch? touch)
    {
        GameObject note = notebase.gameObject;

        onJudge?.Invoke(result);

        if (result == JudgeResult.None)
        {
            Debug.LogWarning("'None' cannot be final judge result. Recognized as 'Miss'.");
            result = JudgeResult.Miss;
        }

        // Tap effect
        int se = (int)EmitEffect(notebase.judgePos, result, notebase.type);

        // Sound effect
        if (notebase.syncLine.PlaySoundEffect(se))
        {
            soundEffects[se].PlayOneShot();
        }

        // Update score
        JudgeResultController.instance.DisplayJudgeResult(result);

        // Update combo
        ComboManager.manager.UpdateCombo(result);

        //Update EL
        JudgeResultController.instance.DisplayJudgeOffset(notebase, (int)result);
    }

    private void UpdateLane(JudgeQueue Q)
    {
        // Remove judged and destroyed notes from queue
        while (!Q.Empty())
        {
            NoteBase obj = Q.Top();
            if (obj.isDestroyed || obj.judgeTime != int.MinValue)
            {
                NotePool.instance.RemoveFromJudgeQueue(obj);
                Q.Pop();
            }
            else
            {
                break;
            }
        }
    }

    private float GetTouchDistance(Touch touch, Vector3 pos)
    {
        Vector3 delta = new Vector3(-1, -1);
        Debug.DrawLine(pos - delta, pos + delta);
        delta.x = 1;
        Debug.DrawLine(pos - delta, pos + delta);
        return 0;
    }

    private NoteBase OnTouch(int audioTime, JudgeQueue Q, Touch touch)
    {
        UpdateLane(Q);
        // Try to judge the front of the queue
        for (int i = 0; i < Q.Count; i++)
        {
            NoteBase note = Q.Get(i);
            if (note.time > audioTime + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
            {
                return null;
            }
            if (NoteUtility.IsSlide(note.type) && (note as SlideNoteBase).IsJudging)
            {
                continue;
            }
            if (note.isFuwafuwa && GetTouchDistance(touch, note.judgePos) > NoteUtility.FUWAFUWA_RADIUS)
            {
                continue;
            }
            JudgeResult result = note.TryJudge(audioTime, touch);
            if (result != JudgeResult.None)
            {
                return note;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    private GameObject CreateNote(GameNoteData gameNote)
    {
        var noteObj = NotePool.instance.GetNote(gameNote.type);
        noteObj.transform.SetParent(transform);
        NoteBase note = noteObj.GetComponent<NoteBase>();
        note.ResetNote(gameNote);
        if (note.isFuwafuwa)
        {
            noteQueue.Push(note.time, note);
        }
        else
        {
            laneQueue[note.lane].Push(note.time, note);
        }
        // Add sync line
        if (!syncTable.ContainsKey(note.time) || syncTable[note.time] == null)
        {
            GameObject syncLineObj = new GameObject("syncLine");
            syncLineObj.layer = 8;
            syncLineObj.transform.SetParent(transform);
            syncTable.Remove(note.time);
            syncTable.Add(note.time, syncLineObj.AddComponent<NoteSyncLine>());
        }
        syncTable[note.time].AddNote(note);

        return noteObj;
    }

    public GameObject CreateSlide(List<GameNoteData> notes)
    {
        GameObject obj = NotePool.instance.GetSlide();
        obj.transform.SetParent(transform);
        Slide slide = obj.GetComponent<Slide>();
        slide.InitSlide();
        foreach (GameNoteData note in notes)
        {
            slide.AddNote(CreateNote(note).GetComponent<NoteBase>());
        }
        slide.FinalizeSlide();
        return obj;
    }

    public static int[] GetLanesByTouchPosition(Vector2 position)
    {
        var ray = mainCamera.ScreenPointToRay(position);
        var cols = Physics.RaycastAll(ray, NoteUtility.NOTE_JUDGE_Z_POS * 4);
        List<int> lanes = new List<int>();
        float[] dists = new float[NoteUtility.LANE_COUNT];
        foreach (var col in cols)
        {
            if (col.collider.CompareTag("JudgeArea"))
            {
                int id = int.Parse(col.collider.name);
                lanes.Add(id);
                dists[id] = Vector3.Distance(col.collider.transform.position, col.point);
            }
            else
            {
                Debug.Log("Touch hit unknown area: " + col.collider.name);
            }
        }
        lanes.Sort((int lhs, int rhs) =>
        {
            return (int)Mathf.Sign(dists[lhs] - dists[rhs]);
        });
        return lanes.ToArray();
    }

    private void UpdateTouch(int audioTime)
    {
        if (LiveSetting.autoPlayEnabled) return;
        audioTime -= LiveSetting.judgeOffset;
        Touch[] touches = Input.touches;
        //var touches = Touch.activeTouches;
        if (touches.Length == 0)
        {
            //Simulate touches with mouse
            if (Input.GetMouseButtonDown(0))
            {
                touches = SimulateMouseTouch(TouchPhase.Began);
            }
            else if (Input.GetMouseButton(0))
            {
                touches = SimulateMouseTouch(TouchPhase.Moved);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                touches = SimulateMouseTouch(TouchPhase.Ended);
            }
        }
        foreach (Touch touch in touches)
        {
            if (touchTable.ContainsKey(touch.fingerId))
            {
                GameObject obj = touchTable[touch.fingerId];
                obj.GetComponent<NoteBase>()?.TraceTouch(audioTime, touch);
                obj.GetComponent<Slide>()?.TraceTouch(audioTime, touch);
                continue;
            }
            // Find lanes
            int[] lanes = GetLanesByTouchPosition(touch.position);
            if (lanes.Length == 0) continue;

            // Find note to judge - non-fuwafuwa
            NoteBase noteToJudge = null;
            NoteBase ret = null;
            foreach (int lane in lanes) {
                ret = OnTouch(audioTime, laneQueue[lane], touch);
                if (ret != null && (noteToJudge == null || noteToJudge.time > ret.time))
                {
                    noteToJudge = ret;
                }
            }
            // Find note to judge - fuwafuwa
            ret = OnTouch(audioTime, noteQueue, touch);
            if (ret != null && (noteToJudge == null || noteToJudge.time > ret.time))
            {
                noteToJudge = ret;
            }
            // A note to judge is not found
            if (noteToJudge == null)
            {
                if (touch.phase == TouchPhase.Began && lanes.Length > 0)
                {
                    int se = (int)EmitEffect(NoteUtility.GetJudgePos(lanes[0]), JudgeResult.None, GameNoteType.Single);
                    soundEffects[se].PlayOneShot();
                }
            }
            else
            {
                noteToJudge.Judge(audioTime, noteToJudge.TryJudge(audioTime, touch), touch);
            }
        }
    }

    private void UpdateNotes(int audioTime)
    {
        while (noteHead < notes.Count)
        {
            GameNoteData note = notes[noteHead];
            if (audioTime <= note.appearTime) break;
            if (note.type == GameNoteType.SlideStart)
            {
                CreateSlide(note.seg);
            }
            else
            {
                CreateNote(note);
            }
            noteHead++;
        }
    }

    void Start()
    {
        instance = this;

        // Main camera
        mainCamera = GameObject.Find("GameMainCamera").GetComponent<Camera>();

        // Init JudgeRange
        NoteUtility.Init(mainCamera.transform.forward);

        // Create tables for fast lookup
        touchTable = new Dictionary<int, GameObject>();
        syncTable = new Dictionary<int, NoteSyncLine>();
        Application.targetFrameRate = 120;

        // Create queue for each lane
        laneQueue = new JudgeQueue[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new JudgeQueue();
        }
        noteQueue = new JudgeQueue();

        // Load chart
        int sid = LiveSetting.CurrentHeader.sid;
        chart = ChartLoader.LoadChart(DataLoader.LoadChart(sid, (Difficulty)LiveSetting.actualDifficulty));
        NotePool.instance.Init(notes);
        noteHead = 0;

        // Compute number of notes
        numNotes = 0;
        foreach (GameNoteData note in notes)
        {
            if (note.type == GameNoteType.SlideStart)
            {
                numNotes += note.seg.Count;
            }
            else
            {
                numNotes++;
            }
        }
        ComboManager.manager.Init(numNotes);

        // Sound effects
        soundEffects = new ISoundEffect[5]
        {
            AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/perfect.wav").bytes),
            AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/great.wav").bytes),
            AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/empty.wav").bytes),
            AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/empty.wav").bytes),
            AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/flick.wav").bytes)
        };

        AudioManager.Instance.DelayPlayInGameBGM(File.ReadAllBytes(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), WARM_UP_SECOND);

        // Background
        var background = GetComponent<InGameBackground>();
        background.SetBcakground(DataLoader.GetBackgroundPath(sid));
        //background = GameObject.Find("dokidokiBackground").GetComponent<FixBackground>();
        //background.UpdateBackground(DataLoader.GetBackgroundPath(sid));

        // Lane Effects
        laneEffects = GameObject.Find("Effects").GetComponent<LaneEffects>();
        laneEffects.Init(chart.bpm, chart.speed);

        //Set Play Mod Event
        //AudioManager.Instance.restart = false;
        onJudge = null;
        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is SuddenDeathMod)
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect && result != JudgeResult.Great)
                    {
                        shutdown = true;
                        AudioManager.Instance.isInGame = false;
                        AudioManager.Instance.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(false);
                    }
                });

            else if (mod is PerfectMod) 
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect)
                    {
                        shutdown = true;
                        AudioManager.Instance.isInGame = false;
                        AudioManager.Instance.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(true);
                    }
                });
        }
    }

    bool shutdown = false;

    void Update()
    {
        if (SceneLoader.Loading || shutdown || Time.timeScale == 0) return;

        int audioTime = AudioTimelineSync.instance.GetTimeInMs() + AudioTimelineSync.RealTimeToBGMTime(LiveSetting.audioOffset);

        // Create notes
        UpdateNotes(audioTime);
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            UpdateLane(laneQueue[i]);
        }
        UpdateLane(noteQueue);

        // Trigger touch event
        UpdateTouch(audioTime);

        // Update each note child
        var noteBase = transform.GetComponentsInChildren<NoteBase>();
        var slide = transform.GetComponentsInChildren<Slide>();
        var noteSyncLine = transform.GetComponentsInChildren<NoteSyncLine>();

        Profiler.BeginSample("OnNoteUpdate");
        for (int i = 0; i < noteBase.Length; i++)
        {
            noteBase[i].OnNoteUpdate(audioTime);
        }
        Profiler.EndSample();

        for (int i = 0; i < slide.Length; i++)
        {
            slide[i].OnSlideUpdate(audioTime);
        }

        for (int i = 0; i < noteSyncLine.Length; i++)
        {
            noteSyncLine[i].OnSyncLineUpdate();
        }

        // Update lane effects
        laneEffects.UpdateLaneEffects(audioTime);
    }
}