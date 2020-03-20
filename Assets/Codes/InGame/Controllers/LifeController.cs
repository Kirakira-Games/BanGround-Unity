using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    public static LifeController instance;
    public static float lifePoint = 100;
    int level;
    Slider lifeSlider;
    Text lifeTxt;
    void Start()
    {
        instance = this;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        UpdateDisplay();
        level = LiveSetting.CurrentHeader.difficultyLevel[LiveSetting.actualDifficulty];
    }

    public void CaculateLife(JudgeResult jr)
    {
        //print(level);
        switch (jr)
        {
            case JudgeResult.Perfect:
                lifePoint += 100 / (level * 3f);
                break;
            case JudgeResult.Great:
                lifePoint += 10 / (level * 3f);
                break;
            case JudgeResult.Good:
                lifePoint -= lifePoint * 0.05f;
                break;
            case JudgeResult.Bad:
                lifePoint -= lifePoint * 0.08f;
                break;
            case JudgeResult.Miss:
                lifePoint -= lifePoint * 0.09f;
                break;
        }
        if (lifePoint < 0.5) lifePoint = 0;
        UpdateDisplay();
    }
    void UpdateDisplay()
    {
        lifeTxt.text = ((int)lifePoint).ToString();
        lifeSlider.value = lifePoint / 100f;
    }
}
