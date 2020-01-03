using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public static NoteController controller;
    private Queue<GameObject>[] laneQueue;
    private Dictionary<int, GameObject> touchTable;
    private List<GameNoteData> notes;
    private int noteHead;

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

    // Judge a note as result
    public void Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        if (result == JudgeResult.None)
        {
            Debug.LogWarning("'None' cannot be final judge result. Recognized as 'Miss'.");
            result = JudgeResult.Miss;
        }
        print(result);
        JudgeResultController.controller.DisplayJudgeResult(result);
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
                if (obj == null || obj.GetComponent<NoteBase>().judgeTime != -1)
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
        if (noteToJudge != null)
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
        note.syncLane = LiveSetting.syncLineEnabled ? gameNote.syncLane : -1;
        note.isGray = LiveSetting.grayNoteEnabled ? gameNote.isGray : false;
        laneQueue[note.lane].Enqueue(noteObj);
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

    private void UpdateTouch()
    {
        int audioTime = (int)(Time.time * 1000);
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

    private void UpdateNotes()
    {
        int audioTime = (int)(Time.time * 1000);
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
        LiveSetting.noteSpeed = 2f;
        Application.targetFrameRate = 120;
        laneQueue = new Queue<GameObject>[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new Queue<GameObject>();
        }
        controller = this;

        notes = ChartLoader.LoadNotesFromFile("TestCharts/0");
        noteHead = 0;
    }

    void Update()
    {
        // Create notes
        UpdateNotes();
        // Trigger touch event
        UpdateTouch();
        // Update each note child
        foreach (Transform child in transform)
        {
            child.GetComponent<NoteBase>()?.OnNoteUpdate();
            child.GetComponent<Slide>()?.OnSlideUpdate();
        }
    }
}
