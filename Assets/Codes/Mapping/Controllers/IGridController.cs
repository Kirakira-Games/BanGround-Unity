using UnityEngine;
using UnityEngine.EventSystems;
using V2;

namespace BGEditor
{
    public interface IGridController
    {
        int EndBar { get; }
        int StartBar { get; }
        Transform transform { get; }

        Vector2 GetLocalPosition(float x, float beat);
        Vector2 GetLocalPosition(float x, int[] beat);
        TimingPoint GetTimingPointByBeat(float beatf);
        void OnPointerClick(PointerEventData eventData);
        void OnSwitchGridView(bool isOn);
        void Refresh();
        T GetComponent<T>();
    }
}