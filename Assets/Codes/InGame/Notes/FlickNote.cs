using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickNote : NoteBase
{
    public override void InitNote()
    {
        base.InitNote();
        var arrow = Instantiate(Resources.Load(LiveSetting.assetDirectory + "/FlickArrow"), transform) as GameObject;
        var ps = arrow.GetComponentInChildren<ParticleSystem>().main;
        ps.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }

    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);
        noteMesh.meshRenderer.sharedMaterial.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_flick_tint"));
        //GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>("note_flick_default");
    }

    public override JudgeResult TryTrace(KirakiraTouch touch)
    {
        if (!TouchManager.TouchesNote(touch.start, this))
            return JudgeResult.None;
        if (touch.duration > NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
            return JudgeResult.Miss;
        if (touch.hasMovedFlickDist)
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.start.time);
        return touch.current.phase == KirakiraTouchPhase.Ended ? JudgeResult.Miss : JudgeResult.None;
    }

    protected override void OnNoteUpdateJudge()
    {
        if (!isTracingOrJudged)
        {
            if (NoteController.judgeTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
            {
                RealJudge(null, JudgeResult.Miss);
            }
        }
        else if (NoteController.judgeTime >
            Mathf.Max(time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad],
                      judgeTime + NoteUtility.SLIDE_END_FLICK_JUDGE_RANGE))
        {
            RealJudge(null, JudgeResult.Miss);
        }
    }

    public override void Judge(KirakiraTouch touch, JudgeResult result)
    {
        TouchManager.instance.RegisterTouch(touch.touchId, this);
        judgeTime = touch.current.time;
    }
}
