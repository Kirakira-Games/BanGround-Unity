using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;

public class NoteController : MonoBehaviour
{
    public static NoteController controller;
    private PriorityQueue<int, NoteBase>[] laneQueue;

    private Dictionary<int, GameObject> touchTable;
    private Dictionary<int, NoteSyncLine> syncTable;
    private List<GameNoteData> notes;
    private int noteHead;
    private int numNotes;
    private AudioManager audioMgr;

    private int[] soundEffects;

    private FixBackground background;

    private UnityAction<JudgeResult> onJudge;

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
        var pos = new Vector3(position.x * 1.444f, -2.97f, 8);

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
    public void Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        onJudge?.Invoke(result);

        NoteBase notebase = note.GetComponent<NoteBase>();
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
            audioMgr.PlaySE(soundEffects[se]);
        }

        // Update score
        JudgeResultController.instance.DisplayJudgeResult(result);

        // Update combo
        ComboManager.manager.UpdateCombo(result);
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

    private void OnTouch(int audioTime, int lane, Touch touch)
    {
        NoteBase noteToJudge = null;
        for (int i = Mathf.Max(0, lane - 1); i < Mathf.Min(NoteUtility.LANE_COUNT, lane + 2); i++)
        {
            UpdateLane(i);
            // Try to judge the front of the queue
            if (!laneQueue[i].Empty())
            {
                NoteBase note = laneQueue[i].Top();
                JudgeResult result = note.TryJudge(audioTime, touch);
                if (result != JudgeResult.None)
                {
                    if (noteToJudge == null || noteToJudge.time > note.time - (i == lane ? 1 : 0))
                    {
                        noteToJudge = note;
                    }
                }
            }
        }
        // A note to judge is found
        if (noteToJudge == null)
        {
            if (touch.phase == TouchPhase.Began)
            {
                int se = (int)EmitEffect(NoteUtility.GetJudgePos(lane), JudgeResult.None, GameNoteType.Single);
                audioMgr.PlaySE(soundEffects[se]);
            }
        }
        else
        {
            noteToJudge.Judge(audioTime, noteToJudge.TryJudge(audioTime, touch), touch);
        }
    }

    private GameObject CreateNote(GameNoteData gameNote)
    {
        var noteObj = NotePool.instance.GetNote(gameNote.type);
        noteObj.transform.SetParent(transform);
        NoteBase note = noteObj.GetComponent<NoteBase>();
        note.time = gameNote.time;
        note.lane = LiveSetting.mirrowEnabled ? NoteUtility.LANE_COUNT - gameNote.lane - 1 : gameNote.lane;
        note.type = gameNote.type;
        note.isGray = LiveSetting.grayNoteEnabled ? gameNote.isGray : false;
        note.anims = gameNote.anims.ToArray();
        note.ResetNote();
        laneQueue[note.lane].Push(note.time, note);
        // Add sync line
        if (!syncTable.ContainsKey(note.time) || syncTable[note.time] == null)
        {
            GameObject syncLineObj = new GameObject("syncLine");
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

    public static int GetLaneByTouchPosition(Vector2 position)
    {
        Collider2D[] cols = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(position));
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("JudgeArea"))
            {
                return col.name[0] - '0';
            }
        }
        return -1;
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
            int lane = GetLaneByTouchPosition(touch.position);
            if (lane != -1)
            {
                OnTouch(audioTime, lane, touch);
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
        controller = this;

        // Load chart
        int sid = LiveSetting.CurrentHeader.sid;
        notes = ChartLoader.LoadChart(DataLoader.LoadChart(sid, (Difficulty)LiveSetting.actualDifficulty));
        noteHead = 0;

        // Compute number of notes
        numNotes = 0;
        foreach (GameNoteData note in notes)
        {
            if (note.type == GameNoteType.SlideStart)
            {
                numNotes += note.seg.Count;
            } else
            {
                numNotes++;
            }
        }
        ComboManager.manager.Init(numNotes);

        // Init JudgeRange
        NoteUtility.InitJudgeRange();

        // Init AudioManager
        audioMgr = AudioManager.Instanse;
        audioMgr.isInGame = true;

        soundEffects = new int[5]
        {
            audioMgr.PrecacheSound(Resources.Load<TextAsset>("SoundEffects/perfect.wav")),
            audioMgr.PrecacheSound(Resources.Load<TextAsset>("SoundEffects/great.wav")),
            audioMgr.PrecacheSound(Resources.Load<TextAsset>("SoundEffects/empty.wav")),
            audioMgr.PrecacheSound(Resources.Load<TextAsset>("SoundEffects/empty.wav")),
            audioMgr.PrecacheSound(Resources.Load<TextAsset>("SoundEffects/flick.wav"))
        };

        audioMgr.DelayPlay(File.ReadAllBytes(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), 4f);

        // Background
        background = GameObject.Find("dokidokiBackground").GetComponent<FixBackground>();
        background.UpdateBackground(DataLoader.GetBackgroundPath(sid));

        //Set Play Mod Event
        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is SuddenDeathMod)
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect && result != JudgeResult.Great)
                    {
                        audioMgr.StopBGM();
                        //audioMgr.restart = true;
                    }
                });

            else if (mod is PerfectMod) 
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect)
                    {
                        audioMgr.StopBGM();
                        audioMgr.restart = true;
                    }
                });
        }
    }

    void Update()
    {
        if (SceneLoader.Loading) return;

        int audioTime = audioMgr.GetBGMPlaybackTime();

        if (audioMgr.GetPauseStatus()) return;

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
    }
}