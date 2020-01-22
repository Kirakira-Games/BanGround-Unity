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

    private Text score_Text;
    private Text score_delta_Text;
    private Text perfect_Text;
    private Text great_Text;
    private Text good_Text;
    private Text bad_Text;
    private Text miss_Text;
    private Text maxCombo_Text;

    private Text level_Text;
    private Text songName_Text;
    private Text acc_Text;

    private Chart chart;
    private Header header;

    private RawImage rankIcon;
    private RawImage markIcon;

    private const string IconPath = "UI/v3/";

    public AudioClip[] voices = new AudioClip[9];

    void Start()
    {
        StartCoroutine(ReadRank());
        SetBtnObject();
        GetResultObjectAndComponent();
        ShowScore();
        ShowRank();
        ShowSongInfo();
    }

    IEnumerator ReadRank()
    {
        yield return new WaitForSeconds(0.8f);
        //print("read");
        AudioSource audioSource = GetComponent<AudioSource>();

        audioSource.clip = voices[0];
        audioSource.Play();

        yield return new WaitForSeconds(1);


        switch (ResultsGetter.GetRanks())
        {
            case Ranks.SSS:
                audioSource.clip = voices[1];
                break;
            case Ranks.SS:
                audioSource.clip = voices[2];
                break;
            case Ranks.S:
                audioSource.clip = voices[3];
                break;
            case Ranks.A:
                audioSource.clip = voices[4];
                break;
            case Ranks.B:
                audioSource.clip = voices[5];
                break;
            case Ranks.C:
                audioSource.clip = voices[6];
                break;
            case Ranks.D:
                audioSource.clip = voices[7];
                break;
            default:
                audioSource.clip = voices[8]; ;
                break;
        }
        audioSource.Play();
        yield return new WaitForSeconds(1);
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
        score_Text = GameObject.Find("Score").GetComponent<Text>();
        score_delta_Text = GameObject.Find("Score_delta").GetComponent<Text>();
        perfect_Text = GameObject.Find("Per_count").GetComponent<Text>();
        great_Text = GameObject.Find("Gre_count").GetComponent<Text>();
        good_Text = GameObject.Find("God_count").GetComponent<Text>();
        bad_Text = GameObject.Find("Bad_count").GetComponent<Text>();
        miss_Text = GameObject.Find("Mis_count").GetComponent<Text>();
        maxCombo_Text = GameObject.Find("Mxm_Comb_count").GetComponent<Text>();

        level_Text = GameObject.Find("Level").GetComponent<Text>();
        songName_Text = GameObject.Find("SongName").GetComponent<Text>();
        acc_Text = GameObject.Find("Acc").GetComponent<Text>();

        rankIcon = GameObject.Find("RankIcon").GetComponent<RawImage>();
        markIcon = GameObject.Find("MarkIcon").GetComponent<RawImage>();
    }

    public void ShowScore()
    {
        score_Text.text = string.Format("{0:0000000}", ComboManager.score / ComboManager.maxScore * 1000000);
        score_delta_Text.text = "Not Implemented";
        perfect_Text.text = ComboManager.judgeCount[(int)JudgeResult.Perfect].ToString();
        great_Text.text = ComboManager.judgeCount[(int)JudgeResult.Great].ToString();
        good_Text.text = ComboManager.judgeCount[(int)JudgeResult.Good].ToString();
        bad_Text.text = ComboManager.judgeCount[(int)JudgeResult.Bad].ToString();
        miss_Text.text = ComboManager.judgeCount[(int)JudgeResult.Miss].ToString();
        maxCombo_Text.text = ComboManager.maxCombo[(int)JudgeResult.Great].ToString();
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

    private void ShowSongInfo()
    {
        chart = ChartLoader.LoadChartFromFile(string.Format(LiveSetting.testChart, LiveSetting.selected));
        header = ChartLoader.LoadHeaderFromFile(string.Format(LiveSetting.testHeader, LiveSetting.selected));
        double acc = ComboManager.acc / (double)ComboManager.maxAcc;

        level_Text.text = Enum.GetName(typeof(Difficulty), chart.difficulty).ToUpper() + " " + chart.level.ToString();
        songName_Text.text = header?.TitleUnicode;
        acc_Text.text = string.Format("{0:P2}", acc);
    }
}

enum ClearMarks { AP,FC,CL,F};
enum Ranks { SSS,SS,S,A,B,C,D,F};
static class ResultsGetter
{
    public static ClearMarks GetClearMark()
    {
        double acc = ComboManager.acc / (double)ComboManager.maxAcc;
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
        double acc = ComboManager.acc / (double)ComboManager.maxAcc;
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
