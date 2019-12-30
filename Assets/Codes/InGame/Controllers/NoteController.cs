using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public static NoteController controller;
    private float lastGenerateTime = -114514f;
    private Queue<GameObject>[] laneQueue;
    private Hashtable touchTable;

    public void RegisterTouch(int id, Object obj)
    {
        touchTable[id] = obj;
    }

    public void UnregisterTouch(int id)
    {
        touchTable.Remove(id);
    }

    // For debugging purpose only, simulate touch event from mouse event
    static private Touch[] SimulateMouseTouch(TouchPhase phase)
    {
        Touch touch = new Touch();
        touch.position = Input.mousePosition;
        touch.fingerId = NoteUtility.MOUSE_TOUCH_ID;
        touch.phase = phase;
        return new Touch[] { touch };
    }

    // Judge a note as result
    public void Judge(GameObject note, JudgeResult result, Touch? touch)
    {
        print(result.ToString());
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
        noteToJudge?.Judge(audioTime, noteToJudge.TryJudge(audioTime, touch), touch);
    }

    private GameObject CreateNote(NoteType type, int time, int lane)
    {
        var noteObj = new GameObject();
        noteObj.transform.SetParent(transform);
        NoteBase note;
        switch (type)
        {
            case NoteType.Single:
                note = noteObj.AddComponent<NoteBase>();
                break;
            case NoteType.Flick:
                note = noteObj.AddComponent<FlickNote>();
                break;
            default:
                Debug.LogWarning("Cannot create noteType: " + type.ToString());
                return null;
        }
        note.time = time;
        note.lane = lane;
        laneQueue[note.lane].Enqueue(noteObj);
        return noteObj;
    }

    private void UpdateTouch()
    {
        int audioTime = (int)(Time.time * 1000);
        Touch[] touches = Input.touches;
        if (Input.touchCount == 0)
        {
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
                (touchTable[touch.fingerId] as GameObject).GetComponent<NoteBase>()?.TraceTouch(audioTime, touch);
                continue;
            }
            Collider2D[] cols = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(touch.position));
            foreach (Collider2D col in cols)
            {
                if (col.tag == "JudgeArea")
                {
                    OnTouch(audioTime, int.Parse(col.name), touch);
                }
            }
        }
    }

    void Start()
    {
        touchTable = new Hashtable();
        LiveSetting.noteSpeed = 10f;
        Application.targetFrameRate = 120;
        laneQueue = new Queue<GameObject>[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new Queue<GameObject>();
        }
        controller = this;
    }

    void Update()
    {
        // Update each note child
        foreach (Transform child in transform)
        {
            child.GetComponent<NoteBase>()?.OnNoteUpdate();
        }

        if (Time.time - lastGenerateTime > 0.5f)
        {
            lastGenerateTime = Time.time;
            int time = (int)((Time.time + 3f) * 1000);
            int lane = Random.Range(0, NoteUtility.LANE_COUNT);
            switch (Random.Range(1, 2))
            {
                case 0:
                    CreateNote(NoteType.Single, time, lane);
                    break;
                case 1:
                    CreateNote(NoteType.Flick, time, lane);
                    break;
            }
        }

        // Trigger touch event
        UpdateTouch();
    }
}
