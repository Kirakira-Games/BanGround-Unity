using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ResultManager : MonoBehaviour
{
    private Text score;
    private Text score_delta;
    private Text perfect;
    private Text great;
    private Text good;
    private Text bad;
    private Text miss;
    private Text maxCombo;

    private RawImage rankIcon;
    private RawImage markIcon;

    void Start()
    {
        GetResultObjectAndComponent();
        ShowScore();
        ShowRank();
    }

    private void GetResultObjectAndComponent()
    {
        score = GameObject.Find("Score").GetComponent<Text>();
        score_delta = GameObject.Find("Score_delta").GetComponent<Text>();
        perfect = GameObject.Find("Per_count").GetComponent<Text>();
        great = GameObject.Find("Gre_count").GetComponent<Text>();
        good = GameObject.Find("God_count").GetComponent<Text>();
        bad = GameObject.Find("Bad_count").GetComponent<Text>();
        miss = GameObject.Find("Mis_count").GetComponent<Text>();
        maxCombo = GameObject.Find("Mxm_Comb_count").GetComponent<Text>();

        rankIcon = GameObject.Find("RankIcon").GetComponent<RawImage>();
        markIcon = GameObject.Find("MarkIcon").GetComponent<RawImage>();
    }

    public void ShowScore()
    {
        score.text = string.Format("{0:0000000}", ComboManager.score / ComboManager.maxScore * 1000000);
        score_delta.text = "Not Implemented";
        perfect.text = ComboManager.judgeCount[(int)JudgeResult.Perfect].ToString();
        great.text = ComboManager.judgeCount[(int)JudgeResult.Great].ToString();
        good.text = ComboManager.judgeCount[(int)JudgeResult.Good].ToString();
        bad.text = ComboManager.judgeCount[(int)JudgeResult.Bad].ToString();
        miss.text = ComboManager.judgeCount[(int)JudgeResult.Miss].ToString();
        maxCombo.text = ComboManager.maxCombo[(int)JudgeResult.Great].ToString();
    }

    private void ShowRank()
    {
        //Set Rank
        double acc = ComboManager.acc / (double)ComboManager.maxAcc;
        Texture2D rank;
        if (acc >= 0.998)
            rank = Resources.Load("UI/icons/SSS") as Texture2D;
        else if (acc >= 0.99)
            rank = Resources.Load("UI/icons/SS") as Texture2D;
        else if (acc >= 0.97)
            rank = Resources.Load("UI/icons/S") as Texture2D;
        else if (acc >= 0.94)
            rank = Resources.Load("UI/icons/A") as Texture2D;
        else if (acc >= 0.90)
            rank = Resources.Load("UI/icons/B") as Texture2D;
        else if (acc >= 0.85)
            rank = Resources.Load("UI/icons/C") as Texture2D;
        else if (acc >= 0.60)
            rank = Resources.Load("UI/icons/D") as Texture2D;
        else
            rank = Resources.Load("UI/icons/Fail") as Texture2D;
        rankIcon.texture = rank;

        //Set Mark
        if (ComboManager.judgeCount[(int)JudgeResult.Perfect] == ComboManager.noteCount)
        {
            markIcon.texture = Resources.Load("UI/icons/AP") as Texture2D;
        }
        else if (ComboManager.maxCombo[(int)JudgeResult.Great] == ComboManager.noteCount) 
        {
            markIcon.texture = Resources.Load("UI/icons/FC") as Texture2D;
        }
        else
        {
            markIcon.texture = null;
        }
    }
}
