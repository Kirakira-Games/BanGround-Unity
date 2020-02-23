using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Collections;

public class NotePool : MonoBehaviour
{
    public static NotePool instance;

    private Queue<GameObject>[] noteQueue;
    private Queue<GameObject> slideQueue;
    private Queue<GameObject>[] teQueue;
    private Object[] tapEffects;
    private static int unitCount;
    private static readonly int[] weight = { 4, 1, 1, 2, 1, 1 };

    private void AddNote(Queue<GameObject> Q, GameNoteType type, int count = 1)
    {
        var name = Enum.GetName(typeof(GameNoteType), type);
        for (int j = 0; j < count; j++)
        {
            var obj = new GameObject(name + j);
            obj.layer = 8;
            NoteBase note = null;
            obj.transform.SetParent(transform);
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
            note.transform.localScale = new Vector3(NoteUtility.NOTE_SCALE, NoteUtility.NOTE_SCALE, 1) * LiveSetting.noteSize;
            note.InitNote();
            if (NoteUtility.IsSlide(type) && !NoteUtility.IsSlideEnd(type))
            {
                var mesh = new GameObject("SlideBody");
                mesh.layer = 8;
                mesh.transform.SetParent(obj.transform);
                mesh.AddComponent<SlideMesh>();
                mesh.AddComponent<MeshRenderer>();
                mesh.AddComponent<MeshFilter>();
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

    void Awake()
    {
        instance = this;

        // Init notemesh
        NoteMesh.Init();
        unitCount = NoteUtility.LANE_COUNT * LiveSetting.NoteScreenTime / 1000;

        noteQueue = new Queue<GameObject>[6];
        for (int i = 0; i < noteQueue.Length; i++)
        {
            noteQueue[i] = new Queue<GameObject>();
            AddNote(noteQueue[i], (GameNoteType)i, unitCount * weight[i]);
        }

        slideQueue = new Queue<GameObject>();
        AddSlide(unitCount);

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
            AddTapEffect(i, NoteUtility.LANE_COUNT);
        }
        AddTapEffect(0, NoteUtility.LANE_COUNT * 4);
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
            AddTapEffect(ty);
        }
        var te = teQueue[ty].Dequeue();
        te.transform.position = pos;
        te.GetComponent<ParticleSystem>().Play();
        te.SetActive(true);
        StartCoroutine(KillFX(te, ty, 0.5f));
    }

    private IEnumerator KillFX(GameObject fx, int type, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        fx.GetComponent<ParticleSystem>().Stop();
        fx.SetActive(false);
        teQueue[type].Enqueue(fx);
    }
}
