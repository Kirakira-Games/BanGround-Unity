using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTick : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(int audioTime, UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        if (audioTime >= time && audioTime <= time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
            return JudgeResult.Perfect;
        return JudgeResult.None;
    }

    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_tick_default"));
    }

    public override void Judge(int audioTime, JudgeResult result, UnityEngine.InputSystem.EnhancedTouch.Touch? touch)
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
