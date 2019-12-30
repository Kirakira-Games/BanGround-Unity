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
        return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, audioTime);
    }

    protected override void Start()
    {
        base.Start();
        sprite.sprite = Resources.Load<Sprite>("V2Assets/note_long_default");
    }

    public override void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        judgeTime = audioTime;
        GetComponentInParent<Slide>().Judge(gameObject, result, touch);
        Destroy(gameObject);
    }
}
