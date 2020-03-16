using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideNoteBase : NoteBase
{
    public bool isTilt;
    public bool isStickEnd;
    public bool isJudging => parentSlide && parentSlide.isJudging;
    public SlideMesh slideMesh;
    public FuwafuwaPillar pillar;
    protected Slide parentSlide;

    protected abstract JudgeResult TrySlideJudge(KirakiraTouch touch);

    public void InitSlideNote()
    {
        isStickEnd = false;
        parentSlide = GetComponentInParent<Slide>();
        if (!NoteUtility.IsSlideEnd(type))
        {
            slideMesh = GetComponentInChildren<SlideMesh>();
            slideMesh.meshRenderer.enabled = true;
        }
        pillar.Init(this);
    }

    public override JudgeResult TryJudge(KirakiraTouch touch)
    {
        if (isTracingOrJudged)
        {
            return JudgeResult.None;
        }
        return TrySlideJudge(touch);
    }

    public override void RealJudge(KirakiraTouch touch, JudgeResult result)
    {
        if (judgeResult != JudgeResult.None) return;
        int ret = parentSlide.Judge(this, result, touch);
        if (ret == 0)
        {
            return;
        }
        judgeTime = touch == null ? NoteController.judgeTime : touch.current.time;
        if (ret == -1)
        {
            judgeResult = JudgeResult.Miss;
        }
        else
        {
            judgeResult = result;
            NoteController.instance.Judge(this, result, touch);
        }
    }
}
