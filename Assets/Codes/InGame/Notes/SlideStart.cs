using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SlideStart : SlideNoteBase
{
    public ParticleSequence tapEffect;

    protected override JudgeResult TrySlideJudge(KirakiraTouch touch)
    {
        if (touch.current.phase != KirakiraTouchPhase.Began)
        {
            return JudgeResult.None;
        }
        if (isTilt)
        {
            return TranslateTimeToJudge(NoteUtility.SLIDE_END_JUDGE_RANGE, touch.current.time);
        }
        else
        {
            return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.current.time);
        }
    }

    public override void InitNote(IResourceLoader resourceLoader, INoteController noteController)
    {
        base.InitNote(resourceLoader, noteController);
        var te = Instantiate(Resources.Load("Effects/effect_spritesheet"), transform) as GameObject;
        tapEffect = te.GetComponent<ParticleSequence>();
        tapEffect.transform.localScale = Vector3.one * r_notesize * NoteUtility.NOTE_SCALE;
    }

    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);
        judgeWindowEnd = time + (isTilt ?
            NoteUtility.SLIDE_END_JUDGE_RANGE :
            NoteUtility.TAP_JUDGE_RANGE)[(int)JudgeResult.Bad];
        noteMesh.meshRenderer.sharedMaterial.SetTexture("_MainTex", resourceLoader.LoadSkinResource<Texture2D>("note_single_tint"));
        //GetComponent<SpriteRenderer>().sprite = resourceLoader.LoadSkinResource<Sprite>("note_long_default");
    }
}
