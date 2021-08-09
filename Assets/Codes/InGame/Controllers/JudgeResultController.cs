using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class JudgeResultController : MonoBehaviour
{
    [Inject]
    private IResourceLoader resourceLoader;

    public static JudgeResultController instance;

    private Sprite[] judges;
    private Sprite early;
    private Sprite late;

    private Text milisecTxt;

    private SpriteRenderer resultRenderer;
    private SpriteRenderer offsetRenderer;
    private Animator animator;
    private OffsetSettingController offsetSetting;

    private void Awake()
    {
        instance = this;
        judges = new Sprite[(int)JudgeResult.Miss + 1];
        for (int i = 0; i <= (int)JudgeResult.Miss; i++)
        {
            judges[i] = resourceLoader.LoadSkinResource<Sprite>("judge_" + i);
        }

        early = resourceLoader.LoadSkinResource<Sprite>("early");
        late = resourceLoader.LoadSkinResource<Sprite>("late");

        animator = GetComponent<Animator>();
        resultRenderer = GetComponent<SpriteRenderer>();
        offsetRenderer = GameObject.Find("JudgeOffset").GetComponent<SpriteRenderer>();
        milisecTxt = GameObject.Find("offset_miliseconds").GetComponent<Text>();
        milisecTxt.text = "";
        var recent10 = GameObject.Find("Recent10Avg");
        if (recent10 != null)
        {
            offsetSetting = recent10.GetComponent<OffsetSettingController>();
        }
    }

    public void DisplayJudgeResult(JudgeResult result)
    {
        resultRenderer.sprite = judges[(int)result];
        animator.Play("Play", -1, 0);
    }

    [Inject(Id = "cl_showms")]
    KVar cl_showms;
    [Inject(Id = "cl_elp")]
    KVar cl_elp;

    public void DisplayJudgeOffset(NoteBase note, int result)
    {
        //if (offsetRenderer == null) return;
        //if (liveSetting.autoPlayEnabled) return;
        if (note is SlideTick)
        {
            offsetRenderer.sprite = null;
            return;
        }

        int deltaTime = note.time - note.judgeTime;
        if (result <= 3)
            offsetSetting?.Add(deltaTime);

        if (cl_showms)
        {
            if (deltaTime >= 0)
            {
                milisecTxt.text = "E" + string.Format("{0:0000}", deltaTime);
                milisecTxt.color = new Color(0, 0.7622f, 1f, 1f);
            }
            else
            {
                milisecTxt.text = "L" + string.Format("{0:0000}", -deltaTime);
                milisecTxt.color = Color.red;
            }
        }
        if (result >= (cl_elp == 0 ? 1 : 0) && result <= 3 && Mathf.Abs(deltaTime) >= cl_elp)
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
        else
        {
            offsetRenderer.sprite = null;
        }
    }

}
public enum OffsetResult { None, Early, Late }

