using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SlideEndFlick : SlideNoteBase
{
    private Vector2 touchPosition;
    protected override JudgeResult TrySlideJudge(int audioTime, TouchState touch)
    {
        if (IsTilt)
        {
            return audioTime >= time ? JudgeResult.Perfect : JudgeResult.None;
        }
        else
        {
            return audioTime >=
                time - NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad] ?
                JudgeResult.Perfect : JudgeResult.None;
        }
    }

    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_flick_default"));
    }

    public override void Judge(int audioTime, JudgeResult result, TouchState? touch)
    {
        touchId = touch.Value.touchId;
        touchPosition = touch.Value.position;
        judgeTime = audioTime;
    }

    public override void TraceTouch(int audioTime, TouchState touch)
    {
        Vector2 dist = touch.position - touchPosition;
        if (dist.magnitude * 2.54F >= Screen.dpi * NoteUtility.FLICK_JUDGE_DIST)
        {
            RealJudge(audioTime, IsTilt ?
                JudgeResult.Perfect :
                TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, audioTime), touch);
        }
        else if (NoteUtility.IsTouchEnd(touch))
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        int judgeEndTime = time + (IsTilt ?
            NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE :
            NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad]);

        if (audioTime > judgeEndTime)
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }
}
