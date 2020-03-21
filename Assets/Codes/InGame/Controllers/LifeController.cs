﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    public static LifeController instance;
    public static List<int> lifePerSecond;
    public float lifePoint { get; private set; }

    public Gradient colors;

    private Image fill;
    private int level;
    private Slider lifeSlider;
    private Text lifeTxt;

    void Start()
    {
        instance = this;
        lifePerSecond = new List<int>();
        lifePoint = 100f;
        lifeSlider = GetComponentInChildren<Slider>();
        lifeTxt = GetComponentInChildren<Text>();
        fill = GameObject.Find("LifeFill").GetComponent<Image>();
        UpdateDisplay();
        level = LiveSetting.CurrentHeader.difficultyLevel[LiveSetting.actualDifficulty];
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
            lifePerSecond.Add((int)lifePoint);
            yield return new WaitForSeconds(5f);
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
