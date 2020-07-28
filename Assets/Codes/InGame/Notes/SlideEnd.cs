using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEnd : SlideNoteBase
{
    public override void UpdatePosition()
    {
        isStickEnd = false;
        if (!parentSlide) Debug.LogWarning(name);
        if (isJudging && NoteController.audioTime >= time)
        {
            isStickEnd = true;
            transform.position = judgePos;
            noteMesh.OnUpdate();
        }
        else
        {
            base.UpdatePosition();
        }
    }

    protected override JudgeResult TrySlideJudge(KirakiraTouch touch)
    {
        if (touch.current.phase != KirakiraTouchPhase.Ended)
        {
            return JudgeResult.None;
        }
        if (isTilt)
        {
            for (int i = 0; i < NoteUtility.SLIDE_END_TILT_JUDGE_RANGE.Length; i++)
            {
                if (touch.current.time >= time + NoteUtility.SLIDE_END_TILT_JUDGE_RANGE[i])
                {
                    return (JudgeResult)i;
                }
            }
            return JudgeResult.None;
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.current.time);
        }
    }

    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);
        noteMesh.meshRenderer.sharedMaterial.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_single_tint"));
        //GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>("note_long_default");
    }

    protected override void OnNoteUpdateJudge()
    {
        if (NoteController.judgeTime > time + (isTilt ?
            NoteUtility.SLIDE_TICK_JUDGE_RANGE :
            NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad]))
        {
            RealJudge(null, JudgeResult.Miss);
        }
    }
}
