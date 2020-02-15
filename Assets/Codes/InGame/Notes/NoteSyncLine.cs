using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSyncLine : MonoBehaviour
{
    public List<GameObject> syncNotes;
    private LineRenderer lineRenderer;
    private const float lineWidth = 0.06f;

    private void Awake()
    {
        syncNotes = new List<GameObject>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = Resources.Load<Material>("TestAssets/Materials/sync_line");
        lineRenderer.startWidth = lineWidth * LiveSetting.noteSize;
        lineRenderer.endWidth = lineWidth * LiveSetting.noteSize;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.rendererPriority = -1;
        lineRenderer.enabled = false;
    }

    public void OnSyncLineUpdate()
    {
        GameObject prevObj = null;
        for (int i = syncNotes.Count - 1; i >= 0; i--)
        {
            GameObject obj = syncNotes[i];
            if (obj == null || obj.GetComponent<NoteBase>().judgeResult != JudgeResult.None)
            {
                syncNotes.RemoveAt(i);
            }
            else if (prevObj == null)
            {
                prevObj = obj;
            }
            else
            {
                float z = prevObj.transform.position.z;
                if (Mathf.Abs(z - obj.transform.position.z) > NoteUtility.EPS)
                {
                    if (z > obj.transform.position.z)
                    {
                        syncNotes.Remove(prevObj);
                        prevObj = obj;
                    }
                    else
                    {
                        syncNotes.RemoveAt(i);
                    }
                }
            }
        }
        if (syncNotes.Count == 0) {
            Destroy(gameObject);
            return;
        }
        if (syncNotes.Count == 1)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        foreach (GameObject obj in syncNotes)
        {
            if (start == Vector3.zero || start.x > obj.transform.position.x)
            {
                start = obj.transform.position;
            }
            if (end == Vector3.zero || end.x < obj.transform.position.x)
            {
                end = obj.transform.position;
            }
        }
        start.z += 0.01f;
        end.z += 0.01f;
        lineRenderer.SetPositions(new Vector3[] { start, end });
    }
}
