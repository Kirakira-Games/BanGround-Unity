using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    public static LifeController instance;

    public float lifePoint { get; private set; }

    private int level;
    private Slider lifeSlider;
    private Text lifeTxt;

    void Start()
    {
        instance = this;
        lifePoint = 100f;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        UpdateDisplay();
        level = LiveSetting.CurrentHeader.difficultyLevel[LiveSetting.actualDifficulty];
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
    void UpdateDisplay()
    {
        lifeTxt.text = Mathf.RoundToInt(lifePoint).ToString();
        lifeSlider.value = lifePoint / 100f;
    }
}
