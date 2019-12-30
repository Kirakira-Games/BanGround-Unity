using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEndFlick : SlideNoteBase
{
    private Vector2 touchPosition;
    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
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

    protected override void Start()
    {
        base.Start();
        sprite.sprite = Resources.Load<Sprite>("V2Assets/note_long_default");
    }

    public override void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        touchId = touch.Value.fingerId;
        touchPosition = touch.Value.position;
        judgeTime = audioTime;
    }

    public override void TraceTouch(int audioTime, Touch touch)
    {
        Vector2 dist = touch.position - touchPosition;
        if (dist.magnitude * 2.54F >= Screen.dpi * NoteUtility.FLICK_JUDGE_DIST)
        {
            base.Judge(audioTime, IsTilt ?
                TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, judgeTime) :
                JudgeResult.Perfect, touch);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            base.Judge(audioTime, JudgeResult.Miss, null);
        }
        return;
    }

    public override void OnNoteUpdate()
    {
        int audioTime = (int)(Time.time * 1000);
        UpdatePosition(audioTime);

        int judgeEndTime = time + (IsTilt ?
            NoteUtility.SLIDE_END_JUDGE_RANGE[(int)JudgeResult.Bad] :
            NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE);
        
        if (touchId == -1 && audioTime > judgeEndTime)
        {
            base.Judge(audioTime, JudgeResult.Miss, null);
        }

        if (audioTime > Mathf.Max(judgeEndTime,
            judgeTime + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE))
        {
            base.Judge(audioTime, JudgeResult.Miss, null);
        }
    }
}
