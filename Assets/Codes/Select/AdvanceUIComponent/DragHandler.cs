using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour//, IBeginDragHandler, IEndDragHandler
{
    //SelectManager_old sm;
    //public bool isDragging = false;
    //public bool canDrag = true;

    //public void Start()
    //{
    //    sm = GameObject.Find("SelectManager").GetComponent<SelectManager_old>();
    //}

    //public void OnBeginDrag(PointerEventData data)
    //{
    //    if (!canDrag) return;
    //    isDragging = true;
    //    sm.UnselectSong();
    //}

    //public void OnEndDrag(PointerEventData data)
    //{
    //    isDragging = false;
    //    sm.SelectSong(-1);
    //    //StartCoroutine(sm.SelectNear());
    //}
}
