using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeResultController : MonoBehaviour
{
    public static JudgeResultController controller;

    private Sprite[] judges;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        controller = this;
        judges = new Sprite[(int)JudgeResult.Miss + 1];
        for (int i = 0; i <= (int)JudgeResult.Miss; i++)
        {
            judges[i] = NoteUtility.LoadResource<Sprite>("judge_" + i);
        }
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DisplayJudgeResult(JudgeResult result)
    {
        spriteRenderer.sprite = judges[(int)result];
        animator.Play("Play", -1, 0);
    }
}
