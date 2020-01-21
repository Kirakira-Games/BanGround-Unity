using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ResultManager : MonoBehaviour
{
    private Button button_back;
    private Button button_retry;

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

    private const string IconPath = "UI/v3/";

    void Start()
    {
        SetBtnObject();
        GetResultObjectAndComponent();
        ShowScore();
        ShowRank();
    }

    private void SetBtnObject()
    {
        button_back = GameObject.Find("Button_back").GetComponent<Button>();
        button_retry = GameObject.Find("Button_retry").GetComponent<Button>();

        Animator anim = GameObject.Find("AnimationManager").GetComponent<Animator>();

        button_back.onClick.AddListener(() =>
        {
            anim.SetBool("FadeToBlue", true);
            StartCoroutine("DelayLoadScene", "Select");
        });

        button_retry.onClick.AddListener(() =>
        {
            anim.SetBool("FadeToBlack", true);
            StartCoroutine("DelayLoadScene","InGame" ); 
        });
    }

    IEnumerator DelayLoadScene(string name)
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync(name);
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
        var rank = new Texture2D(0,0);
        switch (ResultsGetter.GetRanks())
        {
            case Ranks.SSS:
                rank = Resources.Load(IconPath + "SSS") as Texture2D;
                break;
            case Ranks.SS:
                rank = Resources.Load(IconPath + "SS") as Texture2D;
                break;
            case Ranks.S:
                rank = Resources.Load(IconPath + "S") as Texture2D;
                break;
            case Ranks.A:
                rank = Resources.Load(IconPath + "A") as Texture2D;
                break;
            case Ranks.B:
                rank = Resources.Load(IconPath + "B") as Texture2D;
                break;
            case Ranks.C:
                rank = Resources.Load(IconPath + "C") as Texture2D;
                break;
            case Ranks.D:
                rank = Resources.Load(IconPath + "D") as Texture2D;
                break;
            case Ranks.F:
                rank = Resources.Load(IconPath + "F") as Texture2D;
                break;
        }
        rankIcon.texture = rank;

        //Set Mark

        switch (ResultsGetter.GetClearMark())
        {
            case ClearMarks.AP:
                markIcon.texture = Resources.Load(IconPath + "AP") as Texture2D;
                break;
            case ClearMarks.FC:
                markIcon.texture = Resources.Load(IconPath + "FC") as Texture2D;
                break;
            case ClearMarks.CL:
                markIcon.texture = Resources.Load(IconPath + "CL") as Texture2D;
                break;
            case ClearMarks.F:
                markIcon.texture = null;
                break;
        }
    }
}

enum ClearMarks { AP,FC,CL,F};
enum Ranks { SSS,SS,S,A,B,C,D,F};
static class ResultsGetter
{
    static double acc = ComboManager.acc / (double)ComboManager.maxAcc;
    public static ClearMarks GetClearMark()
    {
        if (ComboManager.judgeCount[(int)JudgeResult.Perfect] == ComboManager.noteCount)
        {
            return ClearMarks.AP;
        }
        else if (ComboManager.maxCombo[(int)JudgeResult.Great] == ComboManager.noteCount)
        {
            return ClearMarks.FC;
        }
        else if (acc >= 0.60)
        {
            return ClearMarks.CL;
        }
        else
        {
            return ClearMarks.F; ;
        }
    }
    public static Ranks GetRanks()
    {
        if (acc >= 0.998)
            return Ranks.SSS;
        else if (acc >= 0.99)
            return Ranks.SS;
        else if (acc >= 0.97)
            return Ranks.S;
        else if (acc >= 0.94)
            return Ranks.A;
        else if (acc >= 0.90)
            return Ranks.B;
        else if (acc >= 0.85)
            return Ranks.C;
        else if (acc >= 0.60)
            return Ranks.D;
        else
            return Ranks.F;
    }
}
