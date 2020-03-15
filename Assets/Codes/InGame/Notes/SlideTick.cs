using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTick : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(KirakiraTouch touch)
    {
        if (!isJudging)
        {
            if (touch.current.phase != KirakiraTouchPhase.BEGAN)
                return JudgeResult.None;
            if (touch.current.time >= time - NoteUtility.SLIDE_TICK_JUDGE_RANGE &&
                touch.current.time <= time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
                return JudgeResult.Perfect;
            return JudgeResult.None;
        }
        if (touch.current.time >= time &&
            touch.current.time <= time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
            return JudgeResult.Perfect;
        return JudgeResult.None;
    }

    public override void InitNote()
    {
        base.InitNote();
        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", NoteUtility.LoadResource<Texture2D>("note_tick_default"));
    }

    public override void Judge(KirakiraTouch touch, JudgeResult result)
    {
        RealJudge(touch, result);
    }

    protected override void OnNoteUpdateJudge()
    {
        if (NoteController.judgeTime > time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
        {
            Judge(null, JudgeResult.Miss);
        }
    }
}
