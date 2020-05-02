using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NoteBaseXComparer : IComparer<NoteBase>
{
    public int Compare(NoteBase lhs, NoteBase rhs)
    {
        return (int)Mathf.Sign(lhs.transform.position.x - lhs.transform.position.x);
    }
}

public class NoteSyncLine : MonoBehaviour
{
    private List<NoteBase> syncNotes;
    private List<LineRenderer> syncLines;
    private const float lineWidth = 0.06f;
    private NoteBaseXComparer comparer;
    private int totNotes;
    private bool[] soundEffects;

    static KVarRef r_notesize = new KVarRef("r_notesize");
    private LineRenderer CreateLine()
    {
        GameObject obj = new GameObject("partialSyncLine");
        obj.layer = 8;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(0, -0.05f, 0);
        LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.material = Resources.Load<Material>("InGame/Materials/sync_line");
        lineRenderer.startWidth = lineWidth * r_notesize;
        lineRenderer.endWidth = lineWidth * r_notesize;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.rendererPriority = 1;
        return lineRenderer;
    }

    private void Awake()
    {
        comparer = new NoteBaseXComparer();
        syncNotes = new List<NoteBase>();
        syncLines = new List<LineRenderer>();
        soundEffects = new bool[5];
        totNotes = 0;
    }

    static KVarRef r_syncline = new KVarRef("r_syncline");

    public void OnSyncLineUpdate()
    {
        for (int i = syncNotes.Count - 1; i >= 0; i--)
        {
            NoteBase obj = syncNotes[i];
            if (obj.isDestroyed ||
                obj.judgeResult != JudgeResult.None ||
                obj.GetComponent<SlideNoteBase>()?.isStickEnd == true)
            {
                syncNotes.RemoveAt(i);
            }
        }
        if (totNotes == 0)
        {
            Destroy(gameObject);
            return;
        }
        if (!r_syncline)
        {
            return;
        }
        while (syncLines.Count >= syncNotes.Count && syncLines.Count != 0)
        {
            Destroy(syncLines[syncLines.Count - 1].gameObject);
            syncLines.RemoveAt(syncLines.Count - 1);
        }
        while (syncLines.Count < syncNotes.Count - 1)
        {
            syncLines.Add(CreateLine());
        }
        syncNotes.Sort(comparer);

        for (int i = 0; i < syncLines.Count; i++)
        {
            var pos = new Vector3[2];
            pos[0] = syncNotes[i].transform.position;
            pos[1] = syncNotes[i + 1].transform.position;
            pos[0].z += 0.01f;
            pos[1].z += 0.01f;
            syncLines[i].SetPositions(pos);
        }
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
