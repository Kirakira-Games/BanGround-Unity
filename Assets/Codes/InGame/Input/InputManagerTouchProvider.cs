using UnityEngine;

public class InputManagerTouchProvider : KirakiraTouchProvider
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

    public KirakiraTouchState[] GetTouches()
    {
        var touches = Input.touches;
        KirakiraTouchState[] ret = new KirakiraTouchState[touches.Length];
        for (int i = 0; i < touches.Length; i++)
        {
            var touch = touches[i];
            var ray = NoteController.mainCamera.ScreenPointToRay(touch.position);
            var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

            ret[i] = new KirakiraTouchState
            {
                touchId = touch.fingerId,
                time = NoteController.judgeTime,
                realtime = Time.realtimeSinceStartup,
                screenPos = touch.position,
                pos = pos,
                phase = Kirakira(touch.phase)
            };
        }
        return ret;
    }
}
