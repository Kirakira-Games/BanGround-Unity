using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideNoteBase : NoteBase
{
    public bool IsTilt;
    protected Slide parentSlide;

    protected abstract JudgeResult TrySlideJudge(int audioTime, Touch touch);

    public void InitSlideNote()
    {
        parentSlide = GetComponentInParent<Slide>();
    }

    protected override void OnDestroy()
    {
        int result = (int)judgeResult;
        if (result >= 1 && result <= 3)
        {
            ComboManager.JudgeOffsetResult.Add(time - judgeTime);
            JudgeResultController.controller.DisplayJudgeOffset(time - judgeTime > 0 ? OffsetResult.Early : OffsetResult.Late);
            return;
        }
        JudgeResultController.controller.DisplayJudgeOffset(OffsetResult.None);
    }

    public override JudgeResult TryJudge(int audioTime, Touch touch)
    {
        if (judgeTime != int.MinValue || (parentSlide.GetTouchId() != -1 &&
            parentSlide.GetTouchId() != touch.fingerId))
        {
            return JudgeResult.None;
        }
        return TrySlideJudge(audioTime, touch);
    }

    public override void RealJudge(int audioTime, JudgeResult result, Touch? touch)
    {
        if (judgeResult != JudgeResult.None) return;
        if (parentSlide.Judge(gameObject, result, touch))
        {
            judgeTime = audioTime;
            judgeResult = result;
        }
    }
}
