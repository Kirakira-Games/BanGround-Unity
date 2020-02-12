﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeResultController : MonoBehaviour
{
    public static JudgeResultController controller;

    private Sprite[] judges;
    private Sprite early;
    private Sprite late;

    private SpriteRenderer resultRenderer;
    private SpriteRenderer offsetRenderer;
    private Animator animator;

    private void Awake()
    {
        controller = this;
        judges = new Sprite[(int)JudgeResult.Miss + 1];
        for (int i = 0; i <= (int)JudgeResult.Miss; i++)
        {
            judges[i] = NoteUtility.LoadResource<Sprite>("judge_" + i);
        }

        early = NoteUtility.LoadResource<Sprite>("early"); 
        late = NoteUtility.LoadResource<Sprite>("late");

        animator = GetComponent<Animator>();
        resultRenderer = GetComponent<SpriteRenderer>();
        offsetRenderer=transform.Find("JudgeOffset").GetComponent<SpriteRenderer>();
    }

    public void DisplayJudgeResult(JudgeResult result)
    {
        resultRenderer.sprite = judges[(int)result];
        animator.Play("Play", -1, 0);
    }

    public void DisplayJudgeOffset(OffsetResult result)
    {
        if (offsetRenderer == null) return;
        switch (result)
        {
            case OffsetResult.None:
                offsetRenderer.sprite = null;
                break;
            case OffsetResult.Early:
                offsetRenderer.sprite = early;
                break;
            case OffsetResult.Late:
                offsetRenderer.sprite = late;
                break;
            default:
                break;
        }
    }

}
public enum OffsetResult { None, Early, Late }

