using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEnd : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
    {
        if (!NoteUtility.IsTouchEnd(touch))
        {
            return JudgeResult.None;
        }
        if (IsTilt)
        {
            return TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, audioTime);
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, audioTime);
        }
    }

    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_long_default"));
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + (IsTilt ?
            NoteUtility.SLIDE_END_JUDGE_RANGE :
            NoteUtility.TAP_JUDGE_RANGE)[(int)JudgeResult.Bad])
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }
}
