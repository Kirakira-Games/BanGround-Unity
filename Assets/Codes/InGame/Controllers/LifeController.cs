using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LifeController : MonoBehaviour
{
    [Inject]
    private IChartListManager chartListManager;

    public static LifeController instance;
    public static List<float> lifePerSecond;
    public float lifePoint { get; private set; }

    public Gradient colors;

    private Image fill;
    private int level;
    private Slider lifeSlider;
    private Text lifeTxt;

    void Start()
    {
        instance = this;
        lifePerSecond = new List<float>();
        lifePoint = 100f;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        fill = GameObject.Find("LifeFill").GetComponent<Image>();
        UpdateDisplay();
        level = chartListManager.current.header.difficultyLevel[(int)chartListManager.current.difficulty];
        StartCoroutine(LifeRecorder());
    }

    public void CaculateLife(JudgeResult jr, GameNoteType type)
    {
        if (type == GameNoteType.SlideTick && jr == JudgeResult.Miss)
            jr = JudgeResult.Good;
        //print(level);
        switch (jr)
        {
            case JudgeResult.Perfect:
                lifePoint += 20f / (level + 1);
                break;
            case JudgeResult.Great:
                lifePoint += 4f / (level + 1);
                break;
            case JudgeResult.Good:
                lifePoint -= lifePoint * 0.1f;
                break;
            case JudgeResult.Bad:
                lifePoint -= lifePoint * 0.2f;
                break;
            case JudgeResult.Miss:
                lifePoint -= lifePoint * 0.4f;
                break;
        }
        if (lifePoint < 0.5f) lifePoint = 0;
        if (lifePoint > 100f) lifePoint = 100f;
        UpdateDisplay();
    }

    IEnumerator LifeRecorder()
    {
        while (true)//不知道会不会在场景结束被destroy
        {
            if (UIManager.Instance.SM.Base == GameStateMachine.State.Playing)
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
