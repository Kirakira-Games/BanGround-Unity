using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LifeController : MonoBehaviour
{
    [Inject]
    private IGameStateMachine SM;

    public static LifeController instance;
    public static List<float> lifePerSecond;

    public int lifePoint { get; private set; }
    public float lifePointf => (float)lifePoint / MAX_HP;
    public const int MAX_HP = 1000;

    public int multiplier
    {
        get
        {
            if (lifePoint == MAX_HP)
            {
                return 10;
            }
            else if (lifePoint >= 900)
            {
                return 9;
            }
            else if (lifePoint >= 800)
            {
                return 8;
            }
            else if (lifePoint >= 600)
            {
                return 7;
            }
            else if (lifePoint >= 300)
            {
                return 6;
            }
            else
            {
                return 5;
            }
        }
    }

    public Gradient colors;

    private Image fill;
    private Slider lifeSlider;
    private Text lifeTxt;

    void Start()
    {
        instance = this;
        lifePerSecond = new List<float>();
        lifePoint = MAX_HP;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        fill = GameObject.Find("LifeFill").GetComponent<Image>();
        UpdateDisplay();
        StartCoroutine(LifeRecorder());
    }

    public void CaculateLife(JudgeResult result, GameNoteType type)
    {
        if (type == GameNoteType.SlideTick)
        {
            switch (result)
            {
                case JudgeResult.Miss:
                    lifePoint -= 20;
                    break;
                case JudgeResult.Perfect:
                    lifePoint += 1;
                    break;
            }
        }
        else
        {
            switch (result)
            {
                case JudgeResult.Perfect:
                    lifePoint += 4;
                    break;
                case JudgeResult.Great:
                    lifePoint += 1;
                    break;
                case JudgeResult.Good:
                    break;
                case JudgeResult.Bad:
                    lifePoint -= 50;
                    break;
                case JudgeResult.Miss:
                    lifePoint -= 100;
                    break;
            }
        }
        lifePoint = Mathf.Clamp(lifePoint, 0, MAX_HP);
        UpdateDisplay();
    }

    IEnumerator LifeRecorder()
    {
        while (true)//不知道会不会在场景结束被destroy
        {
            if (SM.Base == GameStateMachine.State.Playing)
                lifePerSecond.Add(lifePointf);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void UpdateDisplay()
    {
        lifeTxt.text = Mathf.RoundToInt(lifePoint).ToString();
        var hpRatio = lifePointf;
        lifeSlider.value = hpRatio;
        fill.color = colors.Evaluate(hpRatio);
        lifeTxt.color = colors.Evaluate(hpRatio);
    }
}
