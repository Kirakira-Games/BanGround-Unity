//using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// For debugging purpose only, simulate touch event from mouse event
public class MouseTouchProvider : IKirakiraTouchProvider
{
    public static KirakiraTouchState[] SimulateMouseTouch(KirakiraTouchPhase phase)
    {
        var mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        var ray = NoteController.mainCamera.ScreenPointToRay(mousePos);
        var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;
        KirakiraTouchState touch = new KirakiraTouchState
        {
            touchId = NoteUtility.MOUSE_TOUCH_ID,
            screenPos = mousePos,
            pos = pos,
            time = NoteController.judgeTime,
            phase = phase
        };
        return new KirakiraTouchState[] { touch };
    }

    public KirakiraTouchState[][] GetTouches()
    {
        KirakiraTouchState[] ret;

        var mouse = UnityEngine.InputSystem.Mouse.current.leftButton;
        if (mouse.wasPressedThisFrame)
        {
            ret = SimulateMouseTouch(KirakiraTouchPhase.Began);
        }
        else if (mouse.isPressed)
        {
            ret = SimulateMouseTouch(KirakiraTouchPhase.Ongoing);
        }
        else if (mouse.wasReleasedThisFrame)
        {
            ret = SimulateMouseTouch(KirakiraTouchPhase.Ended);
        }
        else
        {
            ret = new KirakiraTouchState[0];
        }

        return new KirakiraTouchState[][] { ret };
    }
}
