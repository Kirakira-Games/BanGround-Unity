using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEndFlick : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(KirakiraTouch touch)
    {
        if (isTilt)
        {
            return touch.current.time >= time ? JudgeResult.Perfect : JudgeResult.None;
        }
        else
        {
            return touch.current.time >=
                time - NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad] ?
                JudgeResult.Perfect : JudgeResult.None;
        }
    }

    public override void InitNote()
    {
        base.InitNote();
        GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>("note_flick_default");
        var arrow = Instantiate(Resources.Load(LiveSetting.assetDirectory + "/FlickArrow"), transform) as GameObject;
        var ps = arrow.GetComponentInChildren<ParticleSystem>().main;
        ps.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }

    public override void Judge(KirakiraTouch touch, JudgeResult result)
    {
        judgeTime = touch.current.time;
    }

    public override JudgeResult TryTrace(KirakiraTouch touch)
    {
        if (!isTracingOrJudged) return JudgeResult.None;

        int judgeRange = isTilt ?
            NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE :
            NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad];

        if (touch.timeSinceFlick <= judgeRange)
        {
            return isTilt ? JudgeResult.Perfect :
                TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, touch.current.time);
        }
        else if (touch.current.phase == KirakiraTouchPhase.Ended)
        {
            return JudgeResult.Miss;
        }
        return JudgeResult.None;
    }

    protected override void OnNoteUpdateJudge()
    {
        int judgeEndTime = time + (isTilt ?
            NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE :
            NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad]);

        if (NoteController.judgeTime > judgeEndTime)
        {
            RealJudge(null, JudgeResult.Miss);
        }
    }
}
