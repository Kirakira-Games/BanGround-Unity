using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LifeController : MonoBehaviour
{
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IGameStateMachine SM;

    public static LifeController instance;
    public static List<float> lifePerSecond;
    public int lifePoint { get; private set; }
    public int multiplier
    {
        get
        {
            if (lifePoint > 90)
            {
                return 10;
            }
            else if (lifePoint > 80)
            {
                return 9;
            }
            else if (lifePoint > 60)
            {
                return 8;
            }
            else if (lifePoint > 30)
            {
                return 6;
            }
            else
            {
                return 3;
            }
        }
    }

    public Gradient colors;

    private Image fill;
    private int level;
    private Slider lifeSlider;
    private Text lifeTxt;

    void Start()
    {
        instance = this;
        lifePerSecond = new List<float>();
        lifePoint = 100;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        fill = GameObject.Find("LifeFill").GetComponent<Image>();
        UpdateDisplay();
        level = chartListManager.current.header.difficultyLevel[(int)chartListManager.current.difficulty];
        StartCoroutine(LifeRecorder());
    }

    public void CaculateLife(JudgeResult result, GameNoteType type)
    {
        if (type==GameNoteType.SlideTick)
        {
            if (result == JudgeResult.Miss) lifePoint -= 5;
        }
        else
        {
            switch (result)
            {
                case JudgeResult.Perfect:
                    lifePoint += 1;
                    break;
                case JudgeResult.Great:
                    break;
                case JudgeResult.Good:
                    lifePoint -= 10;
                    break;
                case JudgeResult.Bad:
                    lifePoint -= 20;
                    break;
                case JudgeResult.Miss:
                    lifePoint -= 40;
                    break;
            }
        }
        if (lifePoint < 0) lifePoint = 0;
        if (lifePoint > 100) lifePoint = 100;
        UpdateDisplay();
    }

    IEnumerator LifeRecorder()
    {
        while (true)//不知道会不会在场景结束被destroy
        {
            if (SM.Base == GameStateMachine.State.Playing)
                lifePerSecond.Add(lifePoint / 100.0f);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void UpdateDisplay()
    {
        lifeTxt.text = Mathf.RoundToInt(lifePoint).ToString();
        var inOne = lifePoint / 100f;
        lifeSlider.value = inOne;
        fill.color = colors.Evaluate(inOne);
        lifeTxt.color = colors.Evaluate(inOne);
    }
}
