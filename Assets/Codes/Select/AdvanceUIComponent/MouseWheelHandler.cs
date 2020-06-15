using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseWheelHandler : MonoBehaviour,IScrollHandler
{
    //FUCK:THis wont work after the double mouse game play
    public void OnScroll(PointerEventData p)
    {
        if (p.scrollDelta.y > 0)
        {
            if (LiveSetting.currentChart < DataLoader.chartList.Count-1)
                SelectManager_old.instance.SelectSong(LiveSetting.currentChart + 1);
        }
        else
            SelectManager_old.instance.SelectSong(LiveSetting.currentChart - 1);
    }
}
