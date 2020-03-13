using UnityEngine;
using System.Collections.Generic;

public enum TouchTraceResult
{
    NONE, OK, MISS
}

public class TouchManager : MonoBehaviour
{
    public static TouchManager instance;
    private Dictionary<int, GameObject> touchTable;
    private Dictionary<GameObject, List<int> > exchangable;

    public void RegisterTouch(int id, GameObject obj)
    {
        touchTable[id] = obj;
    }

    public void UnregisterTouch(int id, GameObject obj)
    {
        if (ReferenceEquals(touchTable[id], obj))
        {
            touchTable.Remove(id);
        }
        else
        {
            Debug.LogWarning("Invalid removal from touchTable: " + id);
        }
    }

    public bool IsTracing(int touchId)
    {
        return touchTable.ContainsKey(touchId);
    }

    public void TraceTouch(int audioTime, Touch touch)
    {
        GameObject obj = touchTable[touch.fingerId];
        obj.GetComponent<NoteBase>()?.TraceTouch(audioTime, touch);
        obj.GetComponent<Slide>()?.TraceTouch(audioTime, touch);
    }

    private void Awake()
    {
        instance = this;
        touchTable = new Dictionary<int, GameObject>();
        exchangable = new Dictionary<GameObject, List<int>>();
    }
}
