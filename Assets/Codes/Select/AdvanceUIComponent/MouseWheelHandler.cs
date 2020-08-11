using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class MouseWheelHandler : MonoBehaviour,IScrollHandler
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private ILiveSetting liveSetting;

    //FUCK:THis wont work after the double mouse game play
    public void OnScroll(PointerEventData p)
    {
        if (p.scrollDelta.y > NoteUtility.EPS)
        {
            if (liveSetting.currentChart < dataLoader.chartList.Count-1)
                SelectManager_old.instance.SelectSong(liveSetting.currentChart + 1);
        }
        else if (p.scrollDelta.y < -NoteUtility.EPS)
            SelectManager_old.instance.SelectSong(liveSetting.currentChart - 1);
    }
}
