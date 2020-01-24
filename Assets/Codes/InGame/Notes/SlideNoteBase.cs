using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideNoteBase : NoteBase
{
    public bool IsTilt;

    protected abstract JudgeResult TrySlideJudge(int audioTime, UnityEngine.InputSystem.EnhancedTouch.Touch touch);

    protected override void OnDestroy()
    {
        
    }

    public override JudgeResult TryJudge(int audioTime, UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        if (judgeTime != int.MinValue || (GetComponentInParent<Slide>().GetTouchId() != -1 &&
            GetComponentInParent<Slide>().GetTouchId() != touch.touchId))
        {
            return JudgeResult.None;
        }
        return TrySlideJudge(audioTime, touch);
    }

    public override void RealJudge(int audioTime, JudgeResult result, UnityEngine.InputSystem.EnhancedTouch.Touch? touch)
    {
        if (judgeResult != JudgeResult.None) return;
        if (GetComponentInParent<Slide>().Judge(gameObject, result, touch))
        {
            judgeTime = audioTime;
            judgeResult = result;
        }
    }
}
