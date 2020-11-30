using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class ComboManager : MonoBehaviour
{
    public const string FORMAT_DISPLAY_SCORE = "{0:00000000}";
    public const int MAX_DISPLAY_SCORE = (int)1e7;
    public static string GetDisplayScore(double score) => string.Format(FORMAT_DISPLAY_SCORE, Math.Round(score * MAX_DISPLAY_SCORE));

    public static readonly int[] accRate = { 10, 8, 5, 2, 0 };
    public static int[] maxCombo;
    public static int[] judgeCount;
    public static int score;
    public static int maxScore;
    public static int acc;
    public static int maxAcc;
    public static int noteCount;
    public static ComboManager manager;
    public static List<int> JudgeOffsetResult;

    private int[] combo;

    //[SerializeField] private Material[] comboMat;
    private Animator comboAnimator;
    private GradeColorChange scoreDisplay;
    private Image[] comboImg;
    private Sprite[] comboSprite;
    //private ClearFlag flag = ClearFlag.AP;

    private void Awake()
    {
        maxCombo = new int[2];
        combo = new int[2];
        manager = this;
        score = 0;
        maxScore = 1;
        acc = 0;
        maxAcc = 1;
        judgeCount = new int[(int)JudgeResult.Miss + 1];
        JudgeOffsetResult = new List<int>();
    }

    private void Start()
    {
        scoreDisplay = GameObject.Find("Grades").GetComponent<GradeColorChange>();
        comboAnimator = GameObject.Find("Combos").GetComponent<Animator>();
        comboImg = GameObject.Find("Grid").GetComponentsInChildren<Image>(true);
        comboSprite = Resources.LoadAll<Sprite>("UI/comboCount");
    }

    public void UpdateComboCountAndScore(JudgeResult result)
    {
        int intResult = (int)result;
        judgeCount[intResult]++;
        acc += accRate[intResult];
        maxAcc += accRate[0];
        for (int i = 0; i < combo.Length; i++)
        {
            if (intResult <= i)
            {
                combo[i]++;
                maxCombo[i] = Mathf.Max(maxCombo[i], combo[i]);
                comboAnimator.Play("Play", -1, 0);
            }
            else
            {
                combo[i] = 0;
            }
        }

        score += accRate[intResult] * LifeController.instance.multiplier;
        scoreDisplay.SetScore((double)score / maxScore, (double)acc / maxAcc);

        UpdateComboCountImg();
    }

    private void UpdateComboCountImg()
    {
        int i = comboImg.Length - 1;
        int comboTemp = combo[1];
        while (comboTemp != 0)
        {
            comboImg[i].gameObject.SetActive(true);
            comboImg[i].sprite = comboSprite[comboTemp % 10];
            comboTemp /= 10;
            i--;
        }
        for (; i >= 0; i--)
        {
            comboImg[i].gameObject.SetActive(false);
        }
    }

    public void Init(int numNotes)
    {
        if (numNotes <= 0)
        {
            maxScore = 1;
            maxAcc = 1;
            return;
        }
        noteCount = numNotes;

        maxScore = noteCount * 10 * 10;
    }

    [Flags]
    enum ClearFlag { None, FC, AP }
}
