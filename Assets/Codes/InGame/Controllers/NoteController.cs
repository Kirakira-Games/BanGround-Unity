using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using AudioProvider;

public class NoteController : MonoBehaviour
{
    public static NoteController instance;
    private PriorityQueue<int, NoteBase>[] laneQueue;

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
        var pos = new Vector3(position.x * 1f, 0, 8);

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

        NotePool.instance.PlayTapEffect(se, pos);

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
        int se = (int)EmitEffect(note.transform.position, result, notebase.type);

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

    private void UpdateLane(int i)
    {
        // Remove judged and destroyed notes from queue
        while (!laneQueue[i].Empty())
        {
            NoteBase obj = laneQueue[i].Top();
            if (obj.isDestroyed || obj.judgeTime != int.MinValue)
            {
                NotePool.instance.RemoveFromJudgeQueue(obj);
                laneQueue[i].Pop();
            }
            else
            {
                break;
            }
        }
    }

    private NoteBase OnTouch(int audioTime, int lane, Touch touch)
    {
        UpdateLane(lane);
        // Try to judge the front of the queue
        for (int i = 0; i < laneQueue[lane].Count; i++)
        {
            NoteBase note = laneQueue[lane].Get(i);
            if (NoteUtility.IsSlide(note.type) && (note as SlideNoteBase).IsJudging)
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
        note.time = gameNote.time;
        note.lane = gameNote.lane;
        note.type = gameNote.type;
        note.isGray = LiveSetting.grayNoteEnabled ? gameNote.isGray : false;
        note.anims = gameNote.anims.ToArray();
        note.ResetNote();
        laneQueue[note.lane].Push(note.time, note);
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
        var ray = Camera.main.ScreenPointToRay(position);
        var cols = Physics.RaycastAll(ray, NoteUtility.NOTE_JUDGE_POS * 4);
        List<int> lanes = new List<int>();
        float[] dists = new float[NoteUtility.LANE_COUNT];
        foreach (var col in cols)
        {
            if (col.collider.CompareTag("JudgeArea"))
            {
                int id = int.Parse(col.collider.name);
                lanes.Add(id);
                dists[id] = (col.collider.transform.position - col.point).sqrMagnitude;
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

            // Find note to judge
            NoteBase noteToJudge = null;
            foreach (int lane in lanes) {
                var ret = OnTouch(audioTime, lane, touch);
                if (ret != null && (noteToJudge == null || noteToJudge.time > ret.time))
                {
                    noteToJudge = ret;
                }
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
        touchTable = new Dictionary<int, GameObject>();
        syncTable = new Dictionary<int, NoteSyncLine>();
        Application.targetFrameRate = 120;

        // Create queue for each lane
        laneQueue = new PriorityQueue<int, NoteBase>[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new PriorityQueue<int, NoteBase>();
        }
        instance = this;

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

        // Init JudgeRange
        NoteUtility.InitJudgeRange();

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
        background = GameObject.Find("dokidokiBackground").GetComponent<FixBackground>();
        background.UpdateBackground(DataLoader.GetBackgroundPath(sid));
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
            UpdateLane(i);
        }

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