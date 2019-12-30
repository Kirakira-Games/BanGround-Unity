using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickNote : NoteBase
{
    private Vector2 touchPosition;
    protected override void Start()
    {
        base.Start();
        sprite.sprite = Resources.Load<Sprite>("V2Assets/note_flick_default");
    }

    public override void TraceTouch(int audioTime, Touch touch)
    {
        Vector2 dist = touch.position - touchPosition;
        if (dist.magnitude * 2.54F >= Screen.dpi * NoteUtility.FLICK_JUDGE_DIST)
        {
            base.Judge(audioTime, TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, judgeTime), null);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            base.Judge(audioTime, JudgeResult.Miss, null);
        }
    }

    public override void OnNoteUpdate()
    {
        int audioTime = (int)(Time.time * 1000);
        UpdatePosition(audioTime);

        if (judgeTime == -1)
        {
            if (audioTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
            {
                base.Judge(audioTime, JudgeResult.Miss, null);
            }
        }
        else if (audioTime >
            Mathf.Max(time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad],
                      judgeTime + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE))
        {
            base.Judge(audioTime, JudgeResult.Miss, null);
        }
    }

    public override void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        touchId = touch.Value.fingerId;
        NoteController.controller.RegisterTouch(touchId, gameObject);
        touchPosition = touch.Value.position;
        judgeTime = audioTime;
        return;
    }
}
