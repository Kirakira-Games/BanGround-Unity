using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public static NoteController controller;
    private Queue<GameObject>[] laneQueue;
    private Dictionary<int, GameObject> touchTable;
    private Dictionary<int, NoteSyncLine> syncTable;
    private List<GameNoteData> notes;
    private int noteHead;
    private GradeColorChange scoreDisplay;
    private int numNotes;
    private AudioManager audioMgr;
    private FMOD.Sound SE_PERFECT;
    private FMOD.Sound SE_GREAT;
    private FMOD.Sound SE_GOOD;
    private FMOD.Sound SE_CLICK;
    private FMOD.Sound SE_FLICK;

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

    public void EmitEffect(Vector3 position, JudgeResult result, GameNoteType type)
    {
        if (result == JudgeResult.Miss) return;
        var pos = new Vector3(position.x * 1.444f, -2.97f, 4);
        var effect = "Effects/effect_tap";

        if (NoteUtility.IsFlick(type))
        {
            if (result <= JudgeResult.Good)
                effect += "_swipe";
        }
        else if (result == JudgeResult.Perfect)
            effect += "_perfect";
        else if (result == JudgeResult.Great)
            effect += "_great";
        else if (result == JudgeResult.Good)
            effect += "_good";

        if (effect == "Effects/effect_tap")
            audioMgr.PlaySE(SE_CLICK);
        if (effect == "Effects/effect_tap_swipe")
            audioMgr.PlaySE(SE_FLICK);
        else if (effect == "Effects/effect_tap_perfect")
            audioMgr.PlaySE(SE_PERFECT);
        else if (effect == "Effects/effect_tap_great")
            audioMgr.PlaySE(SE_GREAT);
        else if (effect == "Effects/effect_tap_good")
            audioMgr.PlaySE(SE_GOOD);

        var fx = Instantiate(Resources.Load(effect), pos, Quaternion.identity) as GameObject;
        fx.transform.localScale = Vector3.one * LiveSetting.noteSize * NoteUtility.NOTE_SCALE;
        StartCoroutine(KillFX(fx, 0.5f));
    }

    public static IEnumerator KillFX(GameObject fx, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        Destroy(fx);
    }

    // Judge a note as result
    public void Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        if (result == JudgeResult.None)
        {
            Debug.LogWarning("'None' cannot be final judge result. Recognized as 'Miss'.");
            result = JudgeResult.Miss;
        }

        // Tap effect
        EmitEffect(note.transform.position, result, note.GetComponent<NoteBase>().type);

        // Update score
        JudgeResultController.controller.DisplayJudgeResult(result);

        // Update combo
        ComboManager.manager.UpdateCombo(result);
    }

    private void OnTouch(int audioTime, int lane, Touch touch)
    {
        NoteBase noteToJudge = null;
        for (int i = Mathf.Max(0, lane - 1); i < Mathf.Min(NoteUtility.LANE_COUNT, lane + 2); i++)
        {
            // Remove judged and destroyed notes from queue
            while (laneQueue[i].Count > 0)
            {
                GameObject obj = laneQueue[i].Peek();
                if (obj == null || obj.GetComponent<NoteBase>().judgeTime != int.MinValue)
                {
                    laneQueue[i].Dequeue();
                }
                else
                {
                    break;
                }
            }
            // Try to judge the front of the queue
            if (laneQueue[i].Count > 0)
            {
                NoteBase note = laneQueue[i].Peek().GetComponent<NoteBase>();
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
                EmitEffect(NoteUtility.GetJudgePos(lane), JudgeResult.None, GameNoteType.Normal);
        }
        else
        {
            noteToJudge.Judge(audioTime, noteToJudge.TryJudge(audioTime, touch), touch);
        }
    }

    private GameObject CreateNote(GameNoteData gameNote)
    {
        var noteObj = new GameObject("Note");
        noteObj.transform.SetParent(transform);
        NoteBase note;
        switch (gameNote.type)
        {
            case GameNoteType.Normal:
                note = noteObj.AddComponent<TapNote>();
                break;
            case GameNoteType.Flick:
                note = noteObj.AddComponent<FlickNote>();
                break;
            case GameNoteType.SlideStart:
                note = noteObj.AddComponent<SlideStart>();
                break;
            case GameNoteType.SlideTick:
                note = noteObj.AddComponent<SlideTick>();
                break;
            case GameNoteType.SlideEnd:
                note = noteObj.AddComponent<SlideEnd>();
                break;
            case GameNoteType.SlideEndFlick:
                note = noteObj.AddComponent<SlideEndFlick>();
                break;
            default:
                Debug.LogWarning("Cannot create GameNoteType: " + gameNote.type.ToString());
                return null;
        }
        note.time = gameNote.time;
        note.lane = gameNote.lane;
        note.type = gameNote.type;
        note.isGray = LiveSetting.grayNoteEnabled ? gameNote.isGray : false;
        note.InitNote();
        laneQueue[note.lane].Enqueue(noteObj);
        // Add sync line
        if (LiveSetting.syncLineEnabled && note.type != GameNoteType.SlideTick)
        {
            if (!syncTable.ContainsKey(note.time))
            {
                GameObject syncLineObj = new GameObject("syncLine");
                syncLineObj.transform.SetParent(transform);
                syncTable.Add(note.time, syncLineObj.AddComponent<NoteSyncLine>());
            }
            NoteSyncLine syncLine = syncTable[note.time];
            syncLine.syncNotes.Add(noteObj);
        }
        return noteObj;
    }

    public GameObject CreateSlide(List<GameNoteData> notes)
    {
        GameObject obj = new GameObject("Slide");
        obj.transform.SetParent(transform);
        Slide slide = obj.AddComponent<Slide>();
        slide.InitSlide();
        foreach (GameNoteData note in notes)
        {
            slide.AddNote(CreateNote(note).GetComponent<NoteBase>());
        }
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
        if (Input.touchCount == 0)
        {
            // Simulate touches with mouse
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
                GameObject obj = touchTable[touch.fingerId] as GameObject;
                if (obj.GetComponent<NoteBase>() != null)
                    obj.GetComponent<NoteBase>().TraceTouch(audioTime, touch);
                if (obj.GetComponent<Slide>() != null)
                    obj.GetComponent<Slide>().TraceTouch(audioTime, touch);
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
            int appearTime = note.time - LiveSetting.NoteScreenTime;
            if (audioTime <= appearTime) break;
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
        
        scoreDisplay = GameObject.Find("Grades").GetComponent<GradeColorChange>();
        laneQueue = new Queue<GameObject>[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new Queue<GameObject>();
        }
        controller = this;
        // Load chart
        notes = ChartLoader.LoadNotesFromFile("TestCharts/128");
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

        // Init AudioManager
        audioMgr = GetComponent<AudioManager>();

        SE_PERFECT = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestAssets/SoundEffects/note_perfect.wav"));
        SE_GREAT = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestAssets/SoundEffects/note_great.wav"));
        SE_GOOD = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestAssets/SoundEffects/note_good.wav"));
        SE_FLICK = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestAssets/SoundEffects/note_flick.wav"));
        SE_CLICK = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestAssets/SoundEffects/game_button.wav"));

        var BGM = audioMgr.PrecacheSound(Resources.Load<TextAsset>("TestCharts/bgm128.wav"));
        audioMgr.PlayBGM(BGM);
    }

    void Update()
    {
        int audioTime = audioMgr.GetBGMPlaybackTime();
        // Create notes
        UpdateNotes(audioTime);
        // Trigger touch event
        UpdateTouch(audioTime);
        // Update each note child
        foreach (Transform child in transform)
        {
            child.GetComponent<NoteBase>()?.OnNoteUpdate(audioTime);
            child.GetComponent<Slide>()?.OnSlideUpdate(audioTime);
        }
        foreach (Transform child in transform)
        {
            child.GetComponent<NoteSyncLine>()?.OnSyncLineUpdate();
        }
    }
}