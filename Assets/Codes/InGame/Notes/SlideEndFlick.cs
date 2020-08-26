using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEndFlick : SlideNoteBase
{
    protected FlickArrow flickArrow;

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

    public override void InitNote(IResourceLoader resourceLoader, INoteController noteController)
    {
        base.InitNote(resourceLoader, noteController);
        flickArrow = Instantiate(resourceLoader.LoadResource<GameObject>("FlickArrow"), transform).GetComponent<FlickArrow>();
    }

    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);

        noteMesh.meshRenderer.sharedMaterial.SetTexture("_MainTex", resourceLoader.LoadSkinResource<Texture2D>("note_flick_tint"));
        flickArrow.Reset(timingGroup);
        //GetComponent<SpriteRenderer>().sprite = resourceLoader.LoadSkinResource<Sprite>("note_flick_default");
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

        if (touch.timeSinceFlick <= judgeRange && touch.current.time - touch.timeSinceFlick >= judgeTime + NoteUtility.SLIDE_END_TILT_JUDGE_RANGE[0])
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
