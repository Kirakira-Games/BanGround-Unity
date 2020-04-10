using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using AudioProvider;

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

    private GameObject offset_Obj;
    private Text earlyCount_Text;
    private Text earlyAvg_Text;
    private Text lateCount_Text;
    private Text lateAvg_Text;

    private Text level_Text;
    private Text songName_Text;
    private Text acc_Text;

    private cHeader cheader;
    private mHeader mheader;

    private RawImage rankIcon;
    private RawImage markIcon;
    private Image difficultCard;

    PlayResult playResult = new PlayResult();
    double lastScore = 0;

    public TextAsset[] voices = new TextAsset[9];
    public TextAsset bgmVoice;

    private FixBackground background;
    private ISoundTrack bgmST;

    void Start()
    {
        cheader = LiveSetting.CurrentHeader;
        mheader = DataLoader.GetMusicHeader(cheader.mid);
        
        SetBtnObject();
        GetResultObjectAndComponent();
        ReadScores();
        ShowScore();
        ShowRank();
        ShowSongInfo();
        ShowBackground();
        ShowOffset();
        StartCoroutine(ReadRank());
        bgmST = AudioManager.Instance.PlayLoopMusic(bgmVoice.bytes);
        bgmST.SetVolume(0.7f);
    }

    private void ShowOffset()
    {
        //OffsetList
        var early = ComboManager.JudgeOffsetResult.Where(x => x > 0 && x != int.MinValue);
        var late = ComboManager.JudgeOffsetResult.Where(x => x < 0 && x != int.MaxValue);
        int earlyCount = early.Count();
        int lateCount = late.Count();
        int earlyAverage = earlyCount == 0 ? 0 : Mathf.RoundToInt((float)early.Average());
        int lateAverage = lateCount == 0 ? 0 : Mathf.RoundToInt((float)late.Average());


        //var normal = ComboManager.JudgeOffsetResult.Count - miss - slide;

        //Debug.Log($"total = {ComboManager.JudgeOffsetResult.Count}, early = {earlyCount}, late = {lateCount}");
        //offset_Obj.text = $"E:{earlyCount}(avg:{earlyAverage})\nL:{lateCount}(avg:{lateAverage})";
        earlyCount_Text.text = earlyCount.ToString();
        earlyAvg_Text.text = earlyAverage.ToString() + "ms";
        lateCount_Text.text = lateCount.ToString();
        lateAvg_Text.text = Mathf.Abs(lateAverage).ToString() + "ms";
    }

    private void ShowBackground()
    {
        background = GameObject.Find("Background").GetComponent<FixBackground>();
        string path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        background.UpdateBackground(path);
    }

    IEnumerator ReadRank()
    {
        if (ResultsGetter.GetRanks() != Ranks.F)
        {
            yield return new WaitForSeconds(0.8f);

            var rankPlayer = AudioManager.Instance.PrecacheSE(voices[0].bytes);
            rankPlayer.PlayOneShot();
        }
        TextAsset resultVoice;

        yield return new WaitForSeconds(1);
        

        switch (ResultsGetter.GetRanks())
        {
            case Ranks.SSS:
                resultVoice = voices[1];
                break;
            case Ranks.SS:
                resultVoice = voices[2];
                break;
            case Ranks.S:
                resultVoice = voices[3];
                break;
            case Ranks.A:
                resultVoice = voices[4];
                break;
            case Ranks.B:
                resultVoice = voices[5];
                break;
            case Ranks.C:
                resultVoice = voices[6];
                break;
            case Ranks.D:
                resultVoice = voices[7];
                break;
            default:
                resultVoice = null;
                break;
        }

        if (resultVoice != null)
        {
            var resultPlayer = AudioManager.Instance.PrecacheSE(resultVoice.bytes);
            resultPlayer.PlayOneShot();
        }

        yield return new WaitForSeconds(1.2f);

        TextAsset clearMarkVoice = null;
        TextAsset commentVoice = null;
        var lenth = 0f;

        if (ResultsGetter.GetRanks() <= Ranks.A)
        {
            switch (ResultsGetter.GetClearMark())
            {
                case ClearMarks.AP:
                    clearMarkVoice = voices[19];
                    lenth = 1.8f;
                    commentVoice = voices[12];
                    break;
                case ClearMarks.FC:
                    clearMarkVoice = voices[20];
                    lenth = 2.8f;
                    commentVoice = voices[UnityEngine.Random.Range(10, 12)];
                    break;
                case ClearMarks.CL:
                    //clearMarkVoice = voices[9];
                    //lenth = 0f;
                    commentVoice = voices[UnityEngine.Random.Range(13, 15)];
                    break;
            }
        }
        else
        {
            clearMarkVoice = voices[UnityEngine.Random.Range(15, 18)]; //lp：就这？
        }

        if (clearMarkVoice != null)
        {
            AudioManager.Instance.PrecacheSE(clearMarkVoice.bytes).PlayOneShot();
        }
        yield return new WaitForSeconds(lenth);
        if (commentVoice != null)
        {
            AudioManager.Instance.PrecacheSE(commentVoice.bytes).PlayOneShot();
        }
    }

    private void SetBtnObject()
    {
        button_back = GameObject.Find("Button_back").GetComponent<Button>();
        button_retry = GameObject.Find("Button_retry").GetComponent<Button>();
        Animator anim = GameObject.Find("AnimationManager").GetComponent<Animator>();

        button_back.onClick.AddListener(() =>
        {
            //anim.SetBool("FadeToBlue", true);
            //StartCoroutine("DelayLoadScene", "Select");
            StartCoroutine(BgmFadeOut());
            RemoveListener();
            SceneLoader.LoadScene("Result", "Select", true);
        });

        button_retry.onClick.AddListener(() =>
        {
            //anim.SetBool("FadeToBlack", true);
            //StartCoroutine("DelayLoadScene","InGame" ); 
            StartCoroutine(BgmFadeOut());
            RemoveListener();
            SceneLoader.LoadScene("Result", "InGame",true);
        });

    }

    IEnumerator BgmFadeOut()
    {
        for(float i = 0.7f; i > 0; i -= 0.1f)
        {
            bgmST.SetVolume(i);
            yield return new WaitForSeconds(0.2f);
        }
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

        offset_Obj = GameObject.Find("Offset");
        earlyCount_Text = GameObject.Find("EarlyCount").GetComponent<Text>();
        earlyAvg_Text = GameObject.Find("EarlyAvg").GetComponent<Text>();
        lateCount_Text = GameObject.Find("LateCount").GetComponent<Text>();
        lateAvg_Text = GameObject.Find("LateAvg").GetComponent<Text>();

        level_Text = GameObject.Find("Level").GetComponent<Text>();
        songName_Text = GameObject.Find("SongName").GetComponent<Text>();
        acc_Text = GameObject.Find("Acc").GetComponent<Text>();

        rankIcon = GameObject.Find("RankIcon").GetComponent<RawImage>();
        markIcon = GameObject.Find("MarkIcon").GetComponent<RawImage>();
        difficultCard = GameObject.Find("LevelBG").GetComponent<Image>();

        //offset_Obj.gameObject.SetActive(false);
    }

    public void ShowScore()
    {
        
        score_Text.text = string.Format("{0:0000000}",playResult.Score);
        double delta = playResult.Score - lastScore;
        score_delta_Text.text = string.Format(delta < 0 ? "{0:0000000}": "+{0:0000000}", playResult.Score - lastScore) ;
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
        
        switch (playResult.ranks)
        {
            case Ranks.SSS:
                rank = Resources.Load(LiveSetting.IconPath + "SSS") as Texture2D;
                break;
            case Ranks.SS:
                rank = Resources.Load(LiveSetting.IconPath + "SS") as Texture2D;
                break;
            case Ranks.S:
                rank = Resources.Load(LiveSetting.IconPath + "S") as Texture2D;
                break;
            case Ranks.A:
                rank = Resources.Load(LiveSetting.IconPath + "A") as Texture2D;
                break;
            case Ranks.B:
                rank = Resources.Load(LiveSetting.IconPath + "B") as Texture2D;
                break;
            case Ranks.C:
                rank = Resources.Load(LiveSetting.IconPath + "C") as Texture2D;
                break;
            case Ranks.D:
                rank = Resources.Load(LiveSetting.IconPath + "D") as Texture2D;
                break;
            case Ranks.F:
                rank = Resources.Load(LiveSetting.IconPath + "F") as Texture2D;
                break;
        }
        rankIcon.texture = rank;

        //Set Mark
        
        switch (playResult.clearMark)
        {
            case ClearMarks.AP:
                markIcon.texture = Resources.Load(LiveSetting.IconPath + "AP") as Texture2D;
                break;
            case ClearMarks.FC:
                markIcon.texture = Resources.Load(LiveSetting.IconPath + "FC") as Texture2D;
                break;
            case ClearMarks.CL:
                markIcon.texture = Resources.Load(LiveSetting.IconPath + "CL") as Texture2D;
                break;
            case ClearMarks.F:
                markIcon.texture = null;
                markIcon.color = Color.clear;
                break;
        }
    }

    private void ShowSongInfo()
    {
        level_Text.text = Enum.GetName(typeof(Difficulty), LiveSetting.actualDifficulty).ToUpper() + " " +
            cheader.difficultyLevel[LiveSetting.actualDifficulty];
        songName_Text.text = mheader.title;
        acc_Text.text = LiveSetting.autoPlayEnabled ? "AUTOPLAY" : string.Format("{0:P2}", Mathf.FloorToInt((float)playResult.Acc * 10000) / 10000f);
        difficultCard.sprite = Resources.Load<Sprite>("UI/DifficultyCards/" + Enum.GetName(typeof(Difficulty), LiveSetting.actualDifficulty));
    }

    private void ReadScores()
    {
        float modScoreMultiplier = 1.0f;

        foreach (var mod in LiveSetting.attachedMods)
            modScoreMultiplier *= mod.ScoreMultiplier;

        playResult.Score = (ComboManager.score / ComboManager.maxScore) * 1000000 * modScoreMultiplier;
        playResult.ranks = ResultsGetter.GetRanks();
        playResult.clearMark = ResultsGetter.GetClearMark();
        playResult.Acc = ResultsGetter.GetAcc();
        playResult.ChartId = cheader.sid;
        playResult.Difficulty = (Difficulty)LiveSetting.actualDifficulty;
        PlayRecords pr = PlayRecords.OpenRecord();

        var resultList = pr.resultsList.Where((x) => x.ChartId == cheader.sid && x.Difficulty == (Difficulty)LiveSetting.actualDifficulty);
        if (resultList.Count() == 1) 
        {
            var result = resultList.First();
            lastScore = result.Score;
            if (lastScore < playResult.Score)
            {
                pr.resultsList.Remove(result);
                pr.resultsList.Add(playResult);
            }
        }
        else
        {
            lastScore = 0;
            pr.resultsList.Add(playResult);
        }

        //int count = 0;
        //for(int i =0;i<pr.resultsList.Count;i++)
        //{
        //    if (pr.resultsList[i].FolderName == LiveSetting.selectedFolder && pr.resultsList[i].ChartName == LiveSetting.selectedChart)
        //    {
        //        count++;
        //        lastScore = pr.resultsList[i].Score;
        //        if (lastScore < playResult.Score)
        //        {
        //            pr.resultsList.RemoveAt(i);
        //            pr.resultsList.Add(playResult);
        //        }
        //        break;
        //    }
        //}
        //if (count == 0)
        //{
        //    lastScore = 0;
        //    pr.resultsList.Add(playResult);
        //}
        if (!LiveSetting.autoPlayEnabled)
            print("Record Saved" + PlayRecords.SaveRecord(pr));
        else
            print("Autoplay score not saved");
    }

    private void RemoveListener()
    {
        button_back.onClick.RemoveAllListeners();
        button_retry.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        bgmST.Dispose();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) bgmST?.Play();
        else bgmST?.Pause();
    }
}


static class ResultsGetter
{
    public static ClearMarks GetClearMark()
    {
        double acc = GetAcc();
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
        double acc = GetAcc();
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
    public static double GetAcc()
    {
        double acc = ComboManager.acc / (double)(ComboManager.noteCount * 10);
        return acc;
    }
}
