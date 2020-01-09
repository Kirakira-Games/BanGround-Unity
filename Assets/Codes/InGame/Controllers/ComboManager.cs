﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static readonly int[] accRate = { 10, 8, 5, 2, 0 };
    public static int[] maxCombo;
    public static int[] judgeCount;
    public static double score;
    public static double maxScore;
    public static int acc;
    public static int maxAcc;
    public static ComboManager manager;

    private int[] combo;
    private GradeColorChange scoreDisplay;

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
    }

    private void Start()
    {
        scoreDisplay = GameObject.Find("Grades").GetComponent<GradeColorChange>();
    }

    public void UpdateCombo(JudgeResult result)
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
            }
            else
            {
                combo[i] = 0;
            }
        }
        score += (double)accRate[intResult] / accRate[0] + combo[0] / 50 * 0.005 + combo[1] / 100 * 0.005;
        if (score > maxScore)
        {
            score = maxScore;
        }
        scoreDisplay.SetScore(score / maxScore, (double)acc / maxAcc);
    }

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
        maxScore = numNotes + Accumulate(50, 0.005, numNotes) + Accumulate(100, 0.005, numNotes);
    }

    void Update()
    {
        // TODO
    }
}
