using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideNoteBase : NoteBase
{
    public bool IsTilt;

    protected abstract JudgeResult TrySlideJudge(int audioTime, Touch touch);

    public override JudgeResult TryJudge(int audioTime, Touch touch)
    {
        if (judgeTime != -1 || (GetComponentInParent<Slide>().GetTouchId() != -1 &&
            GetComponentInParent<Slide>().GetTouchId() != touch.fingerId))
        {
            return JudgeResult.None;
        }
        return TrySlideJudge(audioTime, touch);
    }

    public override void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        judgeTime = audioTime;
        GetComponentInParent<Slide>().Judge(gameObject, result, touch);
    }
}
