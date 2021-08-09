using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Collections;
using Zenject;
using BanGround.Game.Mods;

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
    public static NotePool Instance;


    private Queue<NoteBase>[] noteQueue;
    private Queue<Slide> slideQueue;
    private Queue<ParticleSequence> teQueue;
    private Queue<NoteSyncLine> syncLineQueue;
    private Queue<LineRenderer> partialSyncLineQueue;
    private Object tapEffect;

    [Inject]
    private IResourceLoader resourceLoader;
    [Inject]
    private INoteController noteController;
    [Inject(Id = "r_notesize")]
    private KVar r_notesize;
    [Inject(Id = "o_judge")]
    private KVar o_judge;
    [Inject(Id = "r_graynote")]
    private KVar r_graynote;
    [Inject(Id = "r_bang_perspect")]
    private KVar r_bang_perspect;
    [Inject(Id = "r_syncline")]
    KVar r_syncline;

    #region Add
    private void AddNote(Queue<NoteBase> Q, GameNoteType type, int count = 1)
    {
        var name = Enum.GetName(typeof(GameNoteType), type);
        for (int j = 0; j < count; j++)
        {
            var obj = new GameObject(name + j);
            NoteBase note = null;
            obj.transform.SetParent(transform);
            //obj.AddComponent<NoteRotation>();
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
            note.Inject(r_graynote, r_notesize, r_bang_perspect);
            note.isDestroyed = true;

            if (NoteUtility.IsSlide(type))
            {
                var slideNote = note as SlideNoteBase;
                var pillar = new GameObject("Pillar");
                pillar.transform.SetParent(obj.transform);
                slideNote.pillar = pillar.AddComponent<FuwafuwaPillar>();
                slideNote.pillar.Inject(r_notesize);

                if (!NoteUtility.IsSlideEnd(type))
                {
                    var mesh = new GameObject("SlideBody");
                    mesh.transform.SetParent(obj.transform);
                    slideNote.slideMesh = mesh.AddComponent<SlideMesh>();
                    slideNote.slideMesh.InitMesh(resourceLoader);
                    slideNote.slideMesh.Inject(r_notesize);
                    //mesh.AddComponent<NoteRotation>().needRot = false;
                }
            }
            note.InitNote(resourceLoader, noteController);
            obj.SetActive(false);
            Q.Enqueue(note);
        }
    }

    private void AddSyncLine(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("Syncline");
            obj.transform.SetParent(transform);
            var line = obj.AddComponent<NoteSyncLine>();
            syncLineQueue.Enqueue(line);

            line.Inject(r_notesize, r_syncline);
        }
    }

    private void AddPartialSyncLine(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("partialSyncline");
            var renderer = NoteSyncLine.CreatePartialLine(obj);
            obj.transform.SetParent(transform);
            partialSyncLineQueue.Enqueue(renderer);
        }
    }

    private void AddSlide(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("Slide" + i);
            var slide = obj.AddComponent<Slide>();
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            slideQueue.Enqueue(slide);
        }
    }

    private void AddTapEffect(int effect, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var fx = Instantiate(tapEffect, Vector3.zero, Quaternion.identity) as GameObject;
            fx.transform.localScale = Vector3.one * r_notesize * NoteUtility.NOTE_SCALE;
            fx.transform.SetParent(transform);
            teQueue.Enqueue(fx.GetComponent<ParticleSequence>());
        }
    }
    #endregion

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
                    time = range == -1 ?
                        note.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE + 100 + Mathf.Max(0, o_judge) :
                        note.time + range,
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
                time = range == -1 ?
                    note.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE + Mathf.Max(0, o_judge) :
                    note.time + range,
                delta = -1
            });
        }
    }

    public void Init(IModManager modManager, GameNoteData[] notes)
    {
        // Init notemesh
        NoteMesh.Init();

        noteQueue = new Queue<NoteBase>[6];
        for (int i = 0; i < noteQueue.Length; i++)
        {
            noteQueue[i] = new Queue<NoteBase>();
        }

        slideQueue = new Queue<Slide>();

        // Load Tap Effects
        tapEffect = Resources.Load("Effects/effect_spritesheet");
        teQueue = new Queue<ParticleSequence>();

        // Sync lines
        syncLineQueue = new Queue<NoteSyncLine>();
        partialSyncLineQueue = new Queue<LineRenderer>();

        // Compute resource usage
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
        AddPartialSyncLine(total.Max);
        AddSyncLine(total.Max);
        for (int i = 0; i < count.Length; i++)
        {
            AddNote(noteQueue[i], (GameNoteType)i, count[i].Max + 3);
        }
        AddSlide(slide.Max + 3);

        // Tap effects
        events.Clear();
        total.Clear();
        int deltaTime = Mathf.RoundToInt(500 * modManager.SpeedCompensationSum + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE);
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
        for (int i = 0; i < 5; i++)
        {
            AddTapEffect(i, Math.Max(NoteUtility.LANE_COUNT << 1, total.Max) / (i == 0 ? 1 : 2));
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public Slide GetSlide()
    {
        if (slideQueue.Count == 0)
        {
            Debug.Log("Add Slides");
            AddSlide();
        }
        var slide = slideQueue.Dequeue();
        slide.gameObject.SetActive(true);
        return slide;
    }

    public NoteBase GetNote(GameNoteType type)
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
        note.gameObject.SetActive(true);
        return note;
    }

    public LineRenderer GetPartialLine()
    {
        if (partialSyncLineQueue.Count == 0)
        {
            Debug.Log("Add partial line");
            AddPartialSyncLine();
        }
        var renderer = partialSyncLineQueue.Dequeue();
        renderer.gameObject.SetActive(true);
        return renderer;
    }

    public NoteSyncLine GetSyncLine()
    {
        if (syncLineQueue.Count == 0)
        {
            Debug.Log("Add sync line");
            AddSyncLine();
        }
        var line = syncLineQueue.Dequeue();
        line.gameObject.SetActive(true);
        return line;
    }

    public void DestroySlide(Slide slide)
    {
        slide.OnNoteDestroy();
        slide.transform.SetParent(transform);
        slide.gameObject.SetActive(false);
        noteController.OnSlideDestroy(slide);
        slideQueue.Enqueue(slide);
    }

    public void DestroyNote(NoteBase note)
    {
        var type = note.type;
        note.OnNoteDestroy();
        note.transform.SetParent(transform);
        note.isDestroyed = true;
        note.gameObject.SetActive(false);
        if (!note.inJudgeQueue)
        {
            noteController.OnNoteDestroy(note);
            noteQueue[(int)type].Enqueue(note);
        }
    }

    public void DestroySyncLine(NoteSyncLine line)
    {
        line.transform.SetParent(transform);
        line.gameObject.SetActive(false);
        noteController.OnSyncLineDestroy(line);
        syncLineQueue.Enqueue(line);
    }

    public void DestroyPartialSyncLine(LineRenderer line)
    {
        line.transform.SetParent(transform);
        line.gameObject.SetActive(false);
        partialSyncLineQueue.Enqueue(line);
    }

    public void RemoveFromJudgeQueue(NoteBase note)
    {
        note.inJudgeQueue = false;
        if (note.isDestroyed)
        {
            noteController.OnNoteDestroy(note);
            noteQueue[(int)note.type].Enqueue(note);
        }
    }

    public void PlayTapEffect(TapEffectType type, Vector3 pos)
    {
        if (type == TapEffectType.None) return;
        int ty = (int)type;
        if (teQueue.Count == 0)
        {
            Debug.Log("Add TapEffect");
            AddTapEffect(ty);
        }
        var te = teQueue.Dequeue();
        te.transform.position = pos;
        te.Play(type);
        //te.SetActive(true);
        StartCoroutine(KillFX(te, ty, 0.5f));
    }

    private IEnumerator KillFX(ParticleSequence fx, int type, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        //fx.GetComponent<ParticleSystem>().Stop();
        //fx.SetActive(false);
        teQueue.Enqueue(fx);
    }
}
