using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    SelectManager sm;
    public bool isDragging = false;
    public bool canDrag = true;

    public void Start()
    {
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (!canDrag) return;
        isDragging = true;
        sm.UnselectSong();
    }

    public void OnEndDrag(PointerEventData data)
    {
        isDragging = false;
        sm.SelectSong(-1);
    }
}
