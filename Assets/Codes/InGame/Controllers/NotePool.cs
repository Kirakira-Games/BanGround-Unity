using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Collections;

class NoteEvent : IComparable<NoteEvent>
{
    public GameNoteType type;
    public int time;
    public int delta;

    public int CompareTo(NoteEvent other)
    {
        if (time != other.time)
            return time - other.time;
        return other.delta - delta;
    }
}

class NoteEventProcessor
{
    public int Count;
    public int Max;
    public NoteEventProcessor() {
        Clear();
    }
    public void Process(int delta)
    {
        Count += delta;
        Max = Mathf.Max(Max, Count);
    }
    public void Clear()
    {
        Count = Max = 0;
    }
}

public class NotePool : MonoBehaviour
{
    public static NotePool instance;

    private Queue<GameObject>[] noteQueue;
    private Queue<GameObject> slideQueue;
    private Queue<GameObject>[] teQueue;
    private Object[] tapEffects;

    private void AddNote(Queue<GameObject> Q, GameNoteType type, int count = 1)
    {
        var name = Enum.GetName(typeof(GameNoteType), type);
        for (int j = 0; j < count; j++)
        {
            var obj = new GameObject(name + j);
            obj.layer = 8;
            NoteBase note = null;
            obj.transform.SetParent(transform);
            obj.AddComponent<NoteRotation>();
            switch (type)
            {
                case GameNoteType.Single:
                    note = obj.AddComponent<TapNote>();
                    break;
                case GameNoteType.Flick:
                    note = obj.AddComponent<FlickNote>();
                    break;
                case GameNoteType.SlideStart:
                    note = obj.AddComponent<SlideStart>();
                    break;
                case GameNoteType.SlideTick:
                    note = obj.AddComponent<SlideTick>();
                    break;
                case GameNoteType.SlideEnd:
                    note = obj.AddComponent<SlideEnd>();
                    break;
                case GameNoteType.SlideEndFlick:
                    note = obj.AddComponent<SlideEndFlick>();
                    break;
            }
            note.isDestroyed = true;
            note.transform.localScale = new Vector3(NoteUtility.NOTE_SCALE, 1, 1) * LiveSetting.noteSize;
            note.InitNote();
            if (NoteUtility.IsSlide(type) && !NoteUtility.IsSlideEnd(type))
            {
                var mesh = new GameObject("SlideBody");
                mesh.layer = 8;
                mesh.transform.SetParent(obj.transform);
                mesh.AddComponent<SlideMesh>();
                mesh.AddComponent<MeshRenderer>();
                mesh.AddComponent<MeshFilter>();
                mesh.AddComponent<NoteRotation>().needRot = false;
                mesh.AddComponent<SortingGroup>().sortingLayerID = SortingLayer.NameToID("SlideBody");
            }
            if (type == GameNoteType.SlideStart)
            {
                var te = Instantiate(Resources.Load("Effects/effect_TapKeep"), obj.transform) as GameObject;
                te.AddComponent<TapEffect>();
            }
            obj.SetActive(false);
            Q.Enqueue(obj);
        }
    }

    private void AddSlide(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("Slide" + i);
            obj.layer = 8;
            obj.AddComponent<Slide>();
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            slideQueue.Enqueue(obj);
        }
    }

    private void AddTapEffect(int effect, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var fx = Instantiate(tapEffects[effect], Vector3.zero, Quaternion.identity) as GameObject;
            fx.transform.localScale = Vector3.one * LiveSetting.noteSize * NoteUtility.NOTE_SCALE;
            fx.SetActive(false);
            fx.transform.SetParent(transform);
            teQueue[effect].Enqueue(fx);
        }
    }

    private void AddEvent(List<NoteEvent> events, GameNoteData note, int range = -1)
    {
        if (note.seg?.Count > 0)
        {
            foreach (var i in note.seg)
            {
                events.Add(new NoteEvent
                {
                    type = i.type,
                    time = range == -1 ? note.appearTime : note.time - range,
                    delta = 1
                });
                events.Add(new NoteEvent
                {
                    type = i.type,
                    time = range == -1 ? note.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE : note.time + range,
                    delta = -1
                });
            }
        }
        else
        {
            events.Add(new NoteEvent
            {
                type = note.type,
                time = range == -1 ? note.appearTime : note.time - range,
                delta = 1
            });
            events.Add(new NoteEvent
            {
                type = note.type,
                time = range == -1 ? note.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE : note.time + range,
                delta = -1
            });
        }
    }

    public void Init(List<GameNoteData> notes)
    {
        var count = new NoteEventProcessor[(int)GameNoteType.SlideEndFlick + 1];
        for (int i = 0; i < count.Length; i++)
        {
            count[i] = new NoteEventProcessor();
        }
        var slide = new NoteEventProcessor();
        var total = new NoteEventProcessor();
        var events = new List<NoteEvent>();
        // create events
        foreach (var note in notes)
        {
            AddEvent(events, note);
        }
        events.Sort();
        // compute data
        foreach (var e in events) {
            int type = (int)e.type;
            count[type].Process(e.delta);
            total.Process(e.delta);
            if (e.type == GameNoteType.SlideStart && e.delta > 0)
            {
                slide.Process(e.delta);
            }
            if (NoteUtility.IsSlideEnd(e.type) && e.delta < 0)
            {
                slide.Process(e.delta);
            }
        }
        for (int i = 0; i < count.Length; i++)
        {
            AddNote(noteQueue[i], (GameNoteType)i, count[i].Max + 3);
        }
        AddSlide(slide.Max + 3);

        // Tap effects
        events.Clear();
        total.Clear();
        int deltaTime = Mathf.RoundToInt(500 * LiveSetting.SpeedCompensationSum + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE);
        // create events
        foreach (var note in notes)
        {
            AddEvent(events, note, deltaTime);
        }
        events.Sort();
        // compute data
        foreach (var e in events)
        {
            total.Process(e.delta);
        }
        for (int i = 0; i < teQueue.Length; i++)
        {
            AddTapEffect(i, total.Max / (i == 0 ? 1 : 2));
        }
    }

    void Awake()
    {
        instance = this;

        // Init notemesh
        NoteMesh.Init();

        noteQueue = new Queue<GameObject>[6];
        for (int i = 0; i < noteQueue.Length; i++)
        {
            noteQueue[i] = new Queue<GameObject>();
        }

        slideQueue = new Queue<GameObject>();

        // Load Tap Effects
        tapEffects = new Object[]
        {
            Resources.Load("Effects/effect_tap_perfect"),
            Resources.Load("Effects/effect_tap_great"),
            Resources.Load("Effects/effect_tap_good"),
            Resources.Load("Effects/effect_tap"),
            Resources.Load("Effects/effect_tap_swipe")
        };
        teQueue = new Queue<GameObject>[5];
        for (int i = 0; i < teQueue.Length; i++)
        {
            teQueue[i] = new Queue<GameObject>();
        }
    }

    public GameObject GetSlide()
    {
        if (slideQueue.Count == 0)
        {
            Debug.Log("Add Slides");
            AddSlide();
        }
        var slide = slideQueue.Dequeue();
        slide.SetActive(true);
        return slide;
    }

    public GameObject GetNote(GameNoteType type)
    {
        if (type == GameNoteType.None)
        {
            Debug.LogWarning("Cannot create NONE notes");
            return null;
        }
        var Q = noteQueue[(int)type];
        if (Q.Count == 0)
        {
            Debug.Log("Add notes: " + Enum.GetName(typeof(GameNoteType), type));
            AddNote(Q, type);
        }
        var note = Q.Dequeue();
        note.SetActive(true);
        return note;
    }

    public void DestroySlide(GameObject obj)
    {
        obj.GetComponent<Slide>().OnNoteDestroy();
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        slideQueue.Enqueue(obj);
    }

    public void DestroyNote(GameObject obj)
    {
        var note = obj.GetComponent<NoteBase>();
        var type = note.type;
        note.OnNoteDestroy();
        obj.transform.SetParent(transform);
        note.isDestroyed = true;
        obj.SetActive(false);
        if (!note.inJudgeQueue)
        {
            noteQueue[(int)type].Enqueue(obj);
        }
    }

    public void RemoveFromJudgeQueue(NoteBase note)
    {
        note.inJudgeQueue = false;
        if (note.isDestroyed)
        {
            noteQueue[(int)note.type].Enqueue(note.gameObject);
        }
    }

    public void PlayTapEffect(TapEffectType type, Vector3 pos)
    {
        if (type == TapEffectType.None) return;
        int ty = (int)type;
        if (teQueue[ty].Count == 0)
        {
            Debug.Log("Add TapEffect");
            AddTapEffect(ty);
        }
        var te = teQueue[ty].Dequeue();
        te.transform.position = pos;
        te.GetComponent<ParticleSystem>().Play();
        //te.SetActive(true);
        StartCoroutine(KillFX(te, ty, 0.5f));
    }

    private IEnumerator KillFX(GameObject fx, int type, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        fx.GetComponent<ParticleSystem>().Stop();
        //fx.SetActive(false);
        teQueue[type].Enqueue(fx);
    }
}
