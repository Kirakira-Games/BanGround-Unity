using UnityEngine;
using System.Collections.Generic;
using System;

public class NotePool : MonoBehaviour
{
    public static NotePool instance;

    private Queue<GameObject>[] noteQueue;
    private Queue<GameObject> slideQueue;
    private const int INIT_COUNT = NoteUtility.LANE_COUNT;

    private void AddNote(Queue<GameObject> Q, GameNoteType type, int count)
    {
        var name = Enum.GetName(typeof(GameNoteType), type);
        for (int j = 0; j < count; j++)
        {
            var obj = new GameObject(name + j);
            switch (type)
            {
                case GameNoteType.Single:
                    obj.AddComponent<TapNote>();
                    break;
                case GameNoteType.Flick:
                    obj.AddComponent<FlickNote>();
                    break;
                case GameNoteType.SlideStart:
                    obj.AddComponent<SlideStart>();
                    break;
                case GameNoteType.SlideTick:
                    obj.AddComponent<SlideTick>();
                    break;
                case GameNoteType.SlideEnd:
                    obj.AddComponent<SlideEnd>();
                    break;
                case GameNoteType.SlideEndFlick:
                    obj.AddComponent<SlideEndFlick>();
                    break;
            }
            obj.SetActive(false);
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            Q.Enqueue(obj);
        }
    }

    private void AddSlide(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject("Slide" + i);
            obj.AddComponent<Slide>();
            slideQueue.Enqueue(obj);
        }
    }

    void Awake()
    {
        instance = this;
        noteQueue = new Queue<GameObject>[6];
        for (int i = 0; i < noteQueue.Length; i++)
        {
            noteQueue[i] = new Queue<GameObject>();
            AddNote(noteQueue[i], (GameNoteType)i, INIT_COUNT);
        }

        slideQueue = new Queue<GameObject>();
        AddSlide(INIT_COUNT);
    }

    public GameObject GetSlide()
    {
        if (slideQueue.Count == 0)
        {
            Debug.Log("Add Slides");
            AddSlide(INIT_COUNT);
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
            Debug.Log("Add notes");
            AddNote(Q, type, INIT_COUNT);
        }
        var note = Q.Dequeue();
        note.SetActive(true);
        return note;
    }

    public void DestroySlide(GameObject obj)
    {
        obj.GetComponent<Slide>().OnNoteDestroy();
        obj.SetActive(false);
        slideQueue.Enqueue(obj);
    }

    public void DestroyNote(GameObject obj)
    {
        var note = obj.GetComponent<NoteBase>();
        var type = note.type;
        note.OnNoteDestroy();
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
}
