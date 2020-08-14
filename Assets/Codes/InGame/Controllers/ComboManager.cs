using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class ComboManager : MonoBehaviour
{
    public static readonly int[] accRate = { 10, 8, 5, 2, 0 };
    public static int[] maxCombo;
    public static int[] judgeCount;
    public static double score;
    public static double maxScore;
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
        maxScore = 0;
        acc = 0;
        maxAcc = 0;
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

    public void UpdateCombo(JudgeResult result)
    {
        //if (flag == ClearFlag.AP && result > JudgeResult.Perfect) { flag = ClearFlag.FC; UpdateComboMat(); }
        //if (flag == ClearFlag.FC && result > JudgeResult.Great) { flag = ClearFlag.None; UpdateComboMat(); }

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
        double rate = combo[0] / 50 * 0.005 + combo[1] / 100 * 0.005;
        if (rate > 0.1)
        {
            rate = 0.1;
        }
        score += (double)accRate[intResult] / accRate[0] + rate;
        if (score > maxScore)
        {
            score = maxScore;
        }
        scoreDisplay.SetScore(score / maxScore, (double)acc / maxAcc);

        //comboText.text =  combo[1] <= 0 ? "":combo[1].ToString() ;
        UpdateComboImg();
    }

    private void UpdateComboImg()
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

    //private void UpdateComboMat()
    //{
    //    for (int i = 0; i < comboImg.Length; i++)
    //    {
    //        comboImg[i].material = comboMat[(int)flag];
    //    }
    //}

    private static double Accumulate(int segSize, double segDelta, int num)
    {
        int segNum = num / segSize;
        int rest = num % segSize;
        double ans = ((segNum - 1) * segDelta) * segNum * segSize / 2;
        ans += segNum * segDelta * rest;
        return ans;
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

        if (numNotes > 700)
        {
            maxScore = numNotes + Accumulate(50, 0.005, 700) + Accumulate(100, 0.005, 700) + 0.1 * (numNotes - 700);
        }
        else
        {
            maxScore = numNotes + Accumulate(50, 0.005, numNotes) + Accumulate(100, 0.005, numNotes);
        }
    }

    [Flags]
    enum ClearFlag { None,FC,AP}
}
