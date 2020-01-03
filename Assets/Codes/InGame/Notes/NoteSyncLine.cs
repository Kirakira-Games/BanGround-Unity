using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSyncLine
{
    public static void Create(Transform start, int laneDiff)
    {
        Vector3 dist = NoteUtility.GetInitPos(laneDiff) - NoteUtility.GetInitPos(0);
        Vector3 delta = new Vector3(0, 0, 0.1f);
        dist.x /= start.localScale.x;
        LineRenderer lineRenderer = start.gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = Resources.Load<Material>("TestAssets/Materials/sync_line");
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.rendererPriority = -1;
        lineRenderer.SetPositions(new Vector3[]
        {
            delta,
            dist + delta
        });
    }
}
