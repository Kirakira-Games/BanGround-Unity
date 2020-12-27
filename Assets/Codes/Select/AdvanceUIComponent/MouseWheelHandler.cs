using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class MouseWheelHandler : MonoBehaviour,IScrollHandler
{
    //[Inject]
    //private IDataLoader dataLoader;
    //[Inject]
    //private IChartListManager chartListManager;

    //FUCK:THis wont work after the double mouse game play
    public void OnScroll(PointerEventData p)
    {
        //if (p.scrollDelta.y > NoteUtility.EPS)
        //{
        //    if (chartListManager.current.index < dataLoader.chartList.Count-1)
        //        SelectManager_old.instance.SelectSong(chartListManager.current.index + 1);
        //}
        //else if (p.scrollDelta.y < -NoteUtility.EPS)
        //    SelectManager_old.instance.SelectSong(chartListManager.current.index - 1);
    }
}
