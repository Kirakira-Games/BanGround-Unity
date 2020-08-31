using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class LatencyTestButton : Button
{
    public delegate void ButtonDownEventHandler(PointerEventData eventData);
    public event ButtonDownEventHandler OnClick;

    public override void OnPointerDown(PointerEventData eventData)
    {
        OnClick?.Invoke(eventData);
        base.OnPointerDown(eventData);
    }
}