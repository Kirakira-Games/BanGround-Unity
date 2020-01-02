using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public static NoteController controller;
    private float lastGenerateTime = -1f;
    private Queue<GameObject>[] laneQueue;
    private Hashtable touchTable;
    private Hashtable slideTable;

    public void RegisterTouch(int id, Object obj)
    {
        touchTable[id] = obj;
    }

    public void UnregisterTouch(int id, Object obj)
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

    private GameObject CreateNote(GameNoteType type, int time, int lane)
    {
        var noteObj = new GameObject("Note");
        noteObj.transform.SetParent(transform);
        NoteBase note;
        switch (type)
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
                Debug.LogWarning("Cannot create noteType: " + type.ToString());
                return null;
        }
        note.time = time;
        note.lane = lane;
        note.type = type;
        laneQueue[note.lane].Enqueue(noteObj);
        return noteObj;
    }

    public GameObject CreateSlide(int tickStack)
    {
        GameObject obj = new GameObject("Slide");
        obj.transform.SetParent(transform);
        obj.AddComponent<Slide>().InitSlide(tickStack);
        slideTable[tickStack] = obj;
        return obj;
    }

    public Slide GetSlide(int tickStack)
    {   
        return (slideTable[tickStack] as GameObject).GetComponent<Slide>();
    }

    public void EndSlide(int tickStack)
    {
        slideTable.Remove(tickStack);
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
            if (touchTable.Contains(touch.fingerId))
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

    void Start()
    {
        touchTable = new Hashtable();
        slideTable = new Hashtable();
        LiveSetting.noteSpeed = 2f;
        Application.targetFrameRate = 120;
        laneQueue = new Queue<GameObject>[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new Queue<GameObject>();
        }
        controller = this;

        Chart chart = ChartLoader.LoadChartFromFile("TestCharts/0");
        print(chart.authorUnicode);
        print(chart.difficulty);
        print(chart.notes[0].beat[2]);

        /*
        List<int> order = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            order.Add(i);
            CreateSlide(i);
        }
        for (int i = 0; i < 20; i++)
        {
            NoteUtility.Shuffle(order);
            for (int j = 0; j < 7; j++)
            {
                GameObject note;
                if (i == 0)
                {
                    note = CreateNote(GameNoteType.SlideStart, 3000 + i * 200, order[j]);
                }
                else if (i == 19)
                {
                    note = CreateNote(GameNoteType.SlideEndFlick, 3000 + i * 200, order[j]);
                }
                else
                {
                    note = CreateNote(GameNoteType.SlideTick, 3000 + i * 200, order[j]);
                }
                GetSlide(j).AddNote(note.GetComponent<NoteBase>());
            }
        }
        */
        
        CreateSlide(1);
        GameObject[] notes =
        {
            CreateNote(GameNoteType.SlideStart, 2000, 0),
            CreateNote(GameNoteType.SlideTick, 2500, 2),
            CreateNote(GameNoteType.SlideTick, 3200, 0),
            CreateNote(GameNoteType.SlideTick, 3300, 2),
            CreateNote(GameNoteType.SlideTick, 3400, 0),
            CreateNote(GameNoteType.SlideTick, 3500, 2),
            CreateNote(GameNoteType.SlideTick, 3600, 0),
            CreateNote(GameNoteType.SlideTick, 3700, 2),
            CreateNote(GameNoteType.SlideTick, 3800, 0),
            CreateNote(GameNoteType.SlideTick, 3900, 2),
            CreateNote(GameNoteType.SlideEndFlick, 4000, 0)
        };
        foreach (GameObject note in notes)
        {
            GetSlide(1).AddNote(note.GetComponent<NoteBase>());
        }
        
    }

    void Update()
    {
        // Trigger touch event
        UpdateTouch();
        // Update each note child
        foreach (Transform child in transform)
        {
            child.GetComponent<NoteBase>()?.OnNoteUpdate();
            child.GetComponent<Slide>()?.OnSlideUpdate();
        }
        /*
        if (Time.time - lastGenerateTime > 0.5f)
        {
            lastGenerateTime = Time.time;
            int time = (int)((Time.time + 3f) * 1000);
            int lane = Random.Range(0, NoteUtility.LANE_COUNT);
            switch (Random.Range(0, 2))
            {
                case 0:
                    CreateNote(GameNoteType.Normal, time, lane);
                    break;
                case 1:
                    CreateNote(GameNoteType.Flick, time, lane);
                    break;
            }
        }
        */
    }
}
