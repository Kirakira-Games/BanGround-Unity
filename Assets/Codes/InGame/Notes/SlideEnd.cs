using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideEnd : SlideNoteBase
{
    public override void UpdatePosition(int audioTime)
    {
        int timeSub = time - audioTime;

        Vector3 newPos = _cachedInitPos;
        float ratio = 1 - (float)timeSub / LiveSetting.NoteScreenTime;
        if (parentSlide.GetTouchId() != -1)
        {
            ratio = Mathf.Min(1f, ratio);
        }
        if (LiveSetting.bangPerspective)
            ratio = NoteUtility.GetBangPerspective(ratio);
        newPos.z = initPos.z - (NoteUtility.NOTE_START_POS - NoteUtility.NOTE_JUDGE_POS) * ratio;
        transform.position = newPos;
    }

    protected override JudgeResult TrySlideJudge(int audioTime, Touch touch)
    {
        if (!NoteUtility.IsTouchEnd(touch))
        {
            return JudgeResult.None;
        }
        if (IsTilt)
        {
            for (int i = 0; i < NoteUtility.SLIDE_END_TILT_JUDGE_RANGE.Length; i++)
            {
                if (audioTime >= time + NoteUtility.SLIDE_END_TILT_JUDGE_RANGE[i])
                {
                    return (JudgeResult)i;
                }
            }
            return JudgeResult.None;
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, audioTime);
        }
    }

    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_BaseMap", NoteUtility.LoadResource<Texture2D>("note_long_default"));
    }

    protected override void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + (IsTilt ?
            NoteUtility.SLIDE_TICK_JUDGE_RANGE :
            NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad]))
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }
}
