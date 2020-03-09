using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeResultController : MonoBehaviour
{
    public static JudgeResultController instance;

    private Sprite[] judges;
    private Sprite early;
    private Sprite late;

    private SpriteRenderer resultRenderer;
    private SpriteRenderer offsetRenderer;
    private Animator animator;

    private void Awake()
    {
        instance = this;
        judges = new Sprite[(int)JudgeResult.Miss + 1];
        for (int i = 0; i <= (int)JudgeResult.Miss; i++)
        {
            judges[i] = NoteUtility.LoadResource<Sprite>("judge_" + i);
        }

        early = NoteUtility.LoadResource<Sprite>("early"); 
        late = NoteUtility.LoadResource<Sprite>("late");

        animator = GetComponent<Animator>();
        resultRenderer = GetComponent<SpriteRenderer>();
        offsetRenderer = GameObject.Find("JudgeOffset").GetComponent<SpriteRenderer>();
    }

    public void DisplayJudgeResult(JudgeResult result)
    {
        resultRenderer.sprite = judges[(int)result];
        animator.Play("Play", -1, 0);
    }

    public void DisplayJudgeOffset(NoteBase note, int result)
    {
        if (offsetRenderer == null) return;
        if (note is SlideTick) return;

        int deltaTime = note.time - note.judgeTime;
        if (result >= (LiveSetting.displayELP ? 0 : 1) && result <= 3 && deltaTime != 0)
        {
            ComboManager.JudgeOffsetResult.Add(deltaTime);
            OffsetResult offset = deltaTime > 0 ? OffsetResult.Early : OffsetResult.Late;
            switch (offset)
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
                    offsetRenderer.sprite = null;
                    break;
            }
        }

    }

}
public enum OffsetResult { None, Early, Late }

