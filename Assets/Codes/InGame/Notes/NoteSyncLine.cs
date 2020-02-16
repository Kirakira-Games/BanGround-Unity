using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameObjectXComparer : IComparer<GameObject>
{
    public int Compare(GameObject lhs, GameObject rhs)
    {
        return (int)Mathf.Sign(lhs.transform.position.x - lhs.transform.position.x);
    }
}

public class NoteSyncLine : MonoBehaviour
{
    public List<GameObject> syncNotes;
    public List<LineRenderer> syncLines;
    private const float lineWidth = 0.06f;
    private GameObjectXComparer comparer;

    private LineRenderer CreateLine()
    {
        GameObject obj = new GameObject("partialSyncLine");
        obj.transform.SetParent(transform);
        LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = Resources.Load<Material>("TestAssets/Materials/sync_line");
        lineRenderer.startWidth = lineWidth * LiveSetting.noteSize;
        lineRenderer.endWidth = lineWidth * LiveSetting.noteSize;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.rendererPriority = -1;
        return lineRenderer;
    }

    private void Awake()
    {
        comparer = new GameObjectXComparer();
        syncNotes = new List<GameObject>();
        syncLines = new List<LineRenderer>();
    }

    public void OnSyncLineUpdate()
    {
        for (int i = syncNotes.Count - 1; i >= 0; i--)
        {
            GameObject obj = syncNotes[i];
            if (obj == null ||
                obj.GetComponent<NoteBase>().judgeResult != JudgeResult.None ||
                obj.GetComponent<SlideEndFlick>()?.IsStickEnd == true)
            {
                syncNotes.RemoveAt(i);
            }
        }
        if (syncNotes.Count == 0) {
            Destroy(gameObject);
            return;
        }
        while (syncLines.Count >= syncNotes.Count)
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
}
