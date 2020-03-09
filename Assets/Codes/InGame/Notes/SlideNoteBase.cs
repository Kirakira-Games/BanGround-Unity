using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideNoteBase : NoteBase
{
    public bool IsTilt;
    public bool IsStickEnd;
    public bool IsJudging => parentSlide && parentSlide.GetTouchId() != -1;
    public SlideMesh slideMesh;
    protected Slide parentSlide;

    protected abstract JudgeResult TrySlideJudge(int audioTime, Touch touch);

    public void InitSlideNote()
    {
        IsStickEnd = false;
        parentSlide = GetComponentInParent<Slide>();
        if (!NoteUtility.IsSlideEnd(type))
        {
            slideMesh = GetComponentInChildren<SlideMesh>();
            slideMesh.meshRenderer.enabled = true;
        }
    }

    public override void OnNoteDestroy()
    {
        //if (LiveSetting.autoPlayEnabled)
        //{
        //    JudgeResultController.instance.DisplayJudgeOffset(OffsetResult.None);
        //    return;
        //}
        
        //int result = (int)judgeResult;
        //int deltaTime = time - judgeTime;
        //if (result >= (LiveSetting.displayELP ? 0 : 1) && result <= 3 && deltaTime != 0)
        //{
        //    if (Mathf.Abs(deltaTime) > 200)
        //    {
        //        Debug.Log(time + "/" + judgeTime);
        //    }
        //    ComboManager.JudgeOffsetResult.Add(deltaTime);
        //    JudgeResultController.instance.DisplayJudgeOffset(deltaTime > 0 ? OffsetResult.Early : OffsetResult.Late);
        //    return;
        //}
        //JudgeResultController.instance.DisplayJudgeOffset(OffsetResult.None);
    }

    public override JudgeResult TryJudge(int audioTime, Touch touch)
    {
        if (judgeTime != int.MinValue || (IsJudging &&
            parentSlide.GetTouchId() != touch.fingerId))
        {
            return JudgeResult.None;
        }
        return TrySlideJudge(audioTime, touch);
    }

    public override void RealJudge(int audioTime, JudgeResult result, Touch? touch)
    {
        if (judgeResult != JudgeResult.None) return;
        int ret = parentSlide.Judge(this, result, touch);
        if (ret == 1) // judge
        {
            judgeTime = audioTime;
            judgeResult = result;
        }
        else if (ret == -1) // judge miss
        {
            judgeTime = audioTime;
            judgeResult = JudgeResult.Miss;
        }
    }
}
