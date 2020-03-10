using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    SelectManager sm;
    public bool isDragging = false;

    public void Start()
    {
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
    }

    public void OnBeginDrag(PointerEventData data)
    {
        isDragging = true;
        sm.UnselectSong();
    }

    public void OnEndDrag(PointerEventData data)
    {
        isDragging = false;
        sm.SelectSong(-1);
    }
}
