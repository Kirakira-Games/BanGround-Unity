using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideStart : SlideNoteBase
{
    public TapEffect tapEffect;

    protected override JudgeResult TrySlideJudge(KirakiraTouch touch)
    {
        if (touch.current.phase != KirakiraTouchPhase.Began)
        {
            return JudgeResult.None;
        }
        if (isTilt)
        {
            return TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, touch.current.time);
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.current.time);
        }
    }

    public override void InitNote()
    {
        base.InitNote();
        GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>("note_long_default");
    }

    protected override void OnNoteUpdateJudge()
    {
        if (NoteController.judgeTime > time + (isTilt ?
            NoteUtility.SLIDE_END_JUDGE_RANGE:
            NoteUtility.TAP_JUDGE_RANGE)[(int)JudgeResult.Bad])
        {
            RealJudge(null, JudgeResult.Miss);
        }
    }
}
