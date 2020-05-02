using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class InputSystemTouchProvider : KirakiraTouchProvider
{
    public static KirakiraTouchPhase Kirakira(TouchPhase phase)
    {
        switch (phase)
        {
            case TouchPhase.Began:
                return KirakiraTouchPhase.Began;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                return KirakiraTouchPhase.Ongoing;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                return KirakiraTouchPhase.Ended;
            default:
                return KirakiraTouchPhase.None;
        }
    }

    static KVarRef o_judge = new KVarRef("o_judge");

    public KirakiraTouchState[] GetTouches()
    {
        var touches = Touch.activeTouches;
        KirakiraTouchState[] ret = new KirakiraTouchState[touches.Count];
        for (int i = 0; i < touches.Count; i++)
        {
            var touch = touches[i];
            var ray = NoteController.mainCamera.ScreenPointToRay(touch.screenPosition);
            var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

            ret[i] = new KirakiraTouchState
            {
                touchId = touch.touchId,
                time = Mathf.RoundToInt(AudioTimelineSync.instance.TimeSinceStartupToBGMTime((float)touch.time) * 1000) - o_judge,
                realtime = (float)touch.time,
                screenPos = touch.screenPosition,
                pos = pos,
                phase = Kirakira(touch.phase)
            };
        }
        return ret;
    }
}
