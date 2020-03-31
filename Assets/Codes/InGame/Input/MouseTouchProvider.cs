using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// For debugging purpose only, simulate touch event from mouse event
public class MouseTouchProvider : KirakiraTouchProvider
{
    public static KirakiraTouchState[] SimulateMouseTouch(KirakiraTouchPhase phase)
    {
        var ray = NoteController.mainCamera.ScreenPointToRay(Input.mousePosition);
        var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;
        KirakiraTouchState touch = new KirakiraTouchState
        {
            touchId = NoteUtility.MOUSE_TOUCH_ID,
            screenPos = Input.mousePosition,
            pos = pos,
            time = NoteController.judgeTime,
            realtime = Time.realtimeSinceStartup,
            phase = phase
        };
        return new KirakiraTouchState[] { touch };
    }

    public KirakiraTouchState[] GetTouches()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return SimulateMouseTouch(KirakiraTouchPhase.Began);
        }
        if (Input.GetMouseButton(0))
        {
            return SimulateMouseTouch(KirakiraTouchPhase.Ongoing);
        }
        if (Input.GetMouseButtonUp(0))
        {
            return SimulateMouseTouch(KirakiraTouchPhase.Ended);
        }
        return new KirakiraTouchState[0];
    }
}
