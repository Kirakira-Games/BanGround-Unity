using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

class NoteBaseComparer : IComparer<NoteBase>
{
    public int Compare(NoteBase lhs, NoteBase rhs)
    {
        if (Mathf.Approximately(lhs.transform.position.x, rhs.transform.position.x))
            return (int)Mathf.Sign(lhs.transform.position.y - lhs.transform.position.y);
        return (int)Mathf.Sign(lhs.transform.position.x - lhs.transform.position.x);
    }
}

public class NoteSyncLine : MonoBehaviour
{
    public int time;

    private List<NoteBase> syncNotes = new List<NoteBase>();
    private List<LineRenderer> syncLines = new List<LineRenderer>();
    private int totNotes;
    private bool[] soundEffects = new bool[6];

    private static NoteBaseComparer comparer = new NoteBaseComparer();
    private static readonly Vector3 PARTIAL_POS = new Vector3(0, -0.05f, 0);
    private const float lineWidth = 0.06f;

    KVar r_notesize;
    KVar r_syncline;

    public void Inject(KVar r_notesize, KVar r_syncline)
    {
        this.r_notesize = r_notesize;
        this.r_syncline = r_syncline;
    }

    public static LineRenderer CreatePartialLine(GameObject obj)
    {
        obj.layer = 8;
        LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.material = Resources.Load<Material>("InGame/Materials/sync_line");
        lineRenderer.rendererPriority = 1;
        return lineRenderer;
    }

    private void ResetPartialLine(LineRenderer renderer)
    {
        renderer.transform.SetParent(transform);
        renderer.transform.localPosition = PARTIAL_POS;
        float size = r_notesize;
        renderer.startWidth = lineWidth * size;
        renderer.endWidth = lineWidth * size;
    }

    public void ResetLine(int time)
    {
        this.time = time;
        syncNotes.Clear();
        syncLines.Clear();
        for (int i = 0; i < soundEffects.Length; i++) {
            soundEffects[i] = false;
        }
        totNotes = 0;
    }

    public void OnSyncLineUpdate()
    {
        for (int i = syncNotes.Count - 1; i >= 0; i--)
        {
            NoteBase obj = syncNotes[i];
            if (obj.isDestroyed ||
                obj.judgeResult != JudgeResult.None ||
                (obj as SlideNoteBase)?.isStickEnd == true)
            {
                syncNotes.RemoveAt(i);
            }
        }
        if (totNotes == 0)
        {
            foreach (var line in syncLines)
            {
                NotePool.Instance.DestroyPartialSyncLine(line);
            }
            NotePool.Instance.DestroySyncLine(this);
            return;
        }
        if (!r_syncline)
        {
            return;
        }
        while (syncLines.Count >= syncNotes.Count && syncLines.Count != 0)
        {
            NotePool.Instance.DestroyPartialSyncLine(syncLines[syncLines.Count - 1]);
            syncLines.RemoveAt(syncLines.Count - 1);
        }
        while (syncLines.Count < syncNotes.Count - 1)
        {
            var line = NotePool.Instance.GetPartialLine();
            ResetPartialLine(line);
            syncLines.Add(line);
        }
        syncNotes.Sort(comparer);

        for (int i = 0; i < syncLines.Count; i++)
        {
            var prev = syncNotes[i];
            var next = syncNotes[i + 1];
            // Set position
            var pos = new Vector3[2];
            pos[0] = prev.transform.position;
            pos[1] = next.transform.position;
            pos[0].z += 0.01f;
            pos[1].z += 0.01f;
            syncLines[i].SetPositions(pos);
            // Set color
            var color =  Color.white;
            color.a = Mathf.Min(prev.color.a, next.color.a);
            syncLines[i].material.SetColor("_BaseColor", color);
        }
    }

    private void Awake()
    {
        gameObject.layer = 8;
    }

    public void AddNote(NoteBase note)
    {
        totNotes++;
        note.syncLine = this;
        if (note.type == GameNoteType.SlideTick)
        {
            return;
        }
        syncNotes.Add(note);
    }

    public bool PlaySoundEffect(int se)
    {
        totNotes--;
        if (se >= soundEffects.Length || soundEffects[se])
        {
            return false;
        }
        soundEffects[se] = true;
        return true;
    }
}
