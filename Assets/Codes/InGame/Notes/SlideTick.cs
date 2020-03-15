using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTick : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
    {
        if (!IsJudging)
        {
            if (touch.phase != TouchPhase.Began)
                return JudgeResult.None;
            if (audioTime >= time - NoteUtility.SLIDE_TICK_JUDGE_RANGE && audioTime <= time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
                return JudgeResult.Perfect;
            return JudgeResult.None;
        }
        if (audioTime >= time && audioTime <= time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
            return JudgeResult.Perfect;
        return JudgeResult.None;
    }

    public override void OnNoteDestroy() 
    {
        //JudgeResultController.instance.DisplayJudgeOffset(OffsetResult.None);
    }

    public override void InitNote()
    {
        base.InitNote();
        GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>("note_tick_default");
    }

    public override void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        RealJudge(audioTime, result, touch);
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
        {
            Judge(audioTime, JudgeResult.Miss, null);
        }
    }
}
