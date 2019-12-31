using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideStart : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
    {
        if (touch.phase != TouchPhase.Began)
        {
            return JudgeResult.None;
        }
        if (IsTilt)
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, audioTime);
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, audioTime);
        }
    }

    protected override void Start()
    {
        base.Start();
        sprite.sprite = Resources.Load<Sprite>("V2Assets/note_long_default");
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + (IsTilt ?
            NoteUtility.TAP_JUDGE_RANGE :
            NoteUtility.SLIDE_END_JUDGE_RANGE)[(int)JudgeResult.Bad])
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }
}
