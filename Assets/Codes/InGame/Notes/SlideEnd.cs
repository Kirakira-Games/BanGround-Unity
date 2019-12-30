using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEnd : SlideNoteBase
{
    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
    {
        if (touch.phase != TouchPhase.Ended)
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

    public override void OnNoteUpdate()
    {
        int audioTime = (int)(Time.time * 1000);
        UpdatePosition(audioTime);

        if (audioTime > time + (IsTilt ?
            NoteUtility.TAP_JUDGE_RANGE:
            NoteUtility.SLIDE_END_JUDGE_RANGE)[(int)JudgeResult.Bad])
        {
            Judge(audioTime, JudgeResult.Miss, null);
        }
    }
}
