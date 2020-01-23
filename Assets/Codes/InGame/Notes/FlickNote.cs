using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class FlickNote : NoteBase
{
    private Vector2 touchPosition;
    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_flick_default"));
    }

    public override void TraceTouch(int audioTime, TouchState touch)
    {
        Vector2 dist = touch.position - touchPosition;
        if (dist.magnitude * 2.54F >= Screen.dpi * NoteUtility.FLICK_JUDGE_DIST)
        {
            RealJudge(audioTime, TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, judgeTime), touch);
        }
        else if (NoteUtility.IsTouchEnd(touch))
        {
            RealJudge(audioTime, JudgeResult.Miss, touch);
        }
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        if (judgeTime == int.MinValue)
        {
            if (audioTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
            {
                RealJudge(audioTime, JudgeResult.Miss, null);
            }
        }
        else if (audioTime >
            Mathf.Max(time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad],
                      judgeTime + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE))
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }

    public override void Judge(int audioTime, JudgeResult result, TouchState? touch)
    {
        touchId = touch.Value.touchId;
        NoteController.controller.RegisterTouch(touchId, gameObject);
        touchPosition = touch.Value.position;
        judgeTime = audioTime;
    }
}
