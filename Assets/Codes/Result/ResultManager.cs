using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using AudioProvider;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround.Database.Models;
using UnityEngine.Rendering;
using BanGround.Database;
using Newtonsoft.Json;
using BanGround.Scene.Params;
using BanGround.Game.Mods;

public class ResultManager : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IResourceLoader resourceLoader;
    [Inject]
    private IDatabaseAPI db;

    [Inject(Id = "g_demoRecord")]
    private KVar g_demoRecord;
    [Inject(Id = "fs_iconpath")]
    private KVar fs_iconpath;
    [Inject(Id = "cl_currentdemo")]
    private KVar cl_currentdemo;

    private Button button_back;
    private Button button_retry;
    private Button button_replay;

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

    private RankItem playResult = new RankItem();
    private double lastScore = 0;

    public TextAsset[] voices = new TextAsset[9];
    public TextAsset bgmVoice;

    private FixBackground background;
    private ISoundTrack bgmST;

    private ResultParams parameters;

    async void Start()
    {
        parameters = SceneLoader.GetParamsOrDefault<ResultParams>();
        cheader = chartListManager.current.header;
        mheader = dataLoader.GetMusicHeader(cheader.mid);

        SetBtnObject();
        GetResultObjectAndComponent();
        ReadScores();
        ShowScore();
        ShowRank();
        ShowSongInfo();
        ShowBackground();
        ShowOffset();
        ReadRank();
        bgmST = await audioManager.PlayLoopMusic(bgmVoice.bytes);
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
        earlyCount_Text.text ="E"+earlyCount.ToString();
        earlyAvg_Text.text = earlyAverage.ToString() + "ms";
        lateCount_Text.text = "L"+lateCount.ToString();
        lateAvg_Text.text = Mathf.Abs(lateAverage).ToString() + "ms";
    }

    private void ShowBackground()
    {
        background = GameObject.Find("Background").GetComponent<FixBackground>();
        string path = dataLoader.GetBackgroundPath(chartListManager.current.header.sid).Item1;
        background.UpdateBackground(path);
    }

    async void ReadRank()
    {
        if (playResult.Rank != Ranks.F)
        {
            await UniTask.Delay(800);

            var rankPlayer = await audioManager.PrecacheSE(voices[0].bytes);
            rankPlayer.PlayOneShot();
        }
        TextAsset resultVoice;

        await UniTask.Delay(1000);

        switch (playResult.Rank)
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
            var resultPlayer = await audioManager.PrecacheSE(resultVoice.bytes);
            resultPlayer.PlayOneShot();
        }

        await UniTask.Delay(1200);

        TextAsset clearMarkVoice = null;
        TextAsset commentVoice = null;
        var lenth = 0f;

        if (playResult.Rank <= Ranks.A)
        {
            switch (playResult.ClearMark)
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
            (await audioManager.PrecacheSE(clearMarkVoice.bytes)).PlayOneShot();
        }
        await UniTask.Delay((int)(lenth * 1000));
        if (commentVoice != null)
        {
            (await audioManager.PrecacheSE(commentVoice.bytes)).PlayOneShot();
        }
    }

    private void SetBtnObject()
    {
        button_back = GameObject.Find("Button_back").GetComponent<Button>();
        button_retry = GameObject.Find("Button_retry").GetComponent<Button>();
        button_replay = GameObject.Find("Button_watchreplay").GetComponent<Button>();
        Animator anim = GameObject.Find("AnimationManager").GetComponent<Animator>();

        button_back.onClick.AddListener(() =>
        {
            //anim.SetBool("FadeToBlue", true);
            //StartCoroutine("DelayLoadScene", "Select");
            StartCoroutine(BgmFadeOut());
            RemoveListener();
            SceneLoader.Back(null);
        });

        button_retry.onClick.AddListener(() =>
        {
            //anim.SetBool("FadeToBlack", true);
            //StartCoroutine("DelayLoadScene","InGame" ); 
            StartCoroutine(BgmFadeOut());
            RemoveListener();
            SceneLoader.LoadScene("InGame", pushStack: false); // TODO: Add params
        });

        button_replay.onClick.AddListener(() =>
        {
            cl_currentdemo.Set("replay/" + ComboManager.recoder.demoName);
            StartCoroutine(BgmFadeOut());
            RemoveListener();
            SceneLoader.LoadScene("InGame", pushStack: false); // TODO: Add params
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
        score_Text.text = string.Format(ComboManager.FORMAT_DISPLAY_SCORE, playResult.Score);
        double delta = playResult.Score - lastScore;
        score_delta_Text.text = (delta >= 0 ? "+" : "") + string.Format(ComboManager.FORMAT_DISPLAY_SCORE, playResult.Score - lastScore) ;
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
        rankIcon.texture = resourceLoader.LoadIconResource<Texture2D>(playResult.Rank.ToString());

        //Set Mark
        
        switch (playResult.ClearMark)
        {
            case ClearMarks.AP:
                markIcon.texture = resourceLoader.LoadIconResource<Texture2D>("AP") as Texture2D;
                break;
            case ClearMarks.FC:
                markIcon.texture = resourceLoader.LoadIconResource<Texture2D>("FC") as Texture2D;
                break;
            case ClearMarks.CL:
                markIcon.texture = resourceLoader.LoadIconResource<Texture2D>("CL") as Texture2D;
                break;
            case ClearMarks.F:
                markIcon.texture = null;
                markIcon.color = new Color(0,0,0,0);
                break;
        }
    }

    private void ShowSongInfo()
    {
        level_Text.text = Enum.GetName(typeof(Difficulty), chartListManager.current.difficulty).ToUpper() + " " +
            cheader.difficultyLevel[(int)chartListManager.current.difficulty];
        songName_Text.text = mheader.title;
        acc_Text.text = parameters.mods.HasFlag(ModFlag.AutoPlay) ? "AUTOPLAY" : string.Format("{0:P2}", playResult.Acc);
        difficultCard.sprite = Resources.Load<Sprite>("UI/DifficultyCards/" + Enum.GetName(typeof(Difficulty), chartListManager.current.difficulty));
    }

    private void ReadScores()
    {
        playResult.Score = (int)Math.Round((double)ComboManager.score / ComboManager.maxScore * ComboManager.MAX_DISPLAY_SCORE * parameters.scoreMultiplier);
        playResult.Acc = ResultsGetter.GetAcc();
        playResult.ChartId = cheader.sid;
        playResult.MusicId = cheader.mid;
        playResult.Difficulty = chartListManager.current.difficulty;
        playResult.CreatedAt = DateTime.Now;
        playResult.Combo = ResultsGetter.GetCombo();
        playResult.Judge = ResultsGetter.GetJudgeCount();
        playResult.ChartHash = JsonConvert.SerializeObject(chartListManager.ComputeCurrentChartHash());
        playResult.Mods = (ulong)parameters.mods;

        if (g_demoRecord)
        {
            playResult.ReplayFile = "replay/" + ComboManager.recoder.demoName;
        }

        var oldBest = db.GetBestRank(cheader.sid, chartListManager.current.difficulty);
        lastScore = oldBest?.Score ?? 0;

        if (parameters.saveRecord)
        {
            db.SaveRankItem(playResult);
            print("Record saved");
        }
        else
        {
            print("Autoplay score not saved");
            cl_currentdemo.Set("");
        }
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
    public static int GetCombo()
    {
        return ComboManager.maxCombo[(int)JudgeResult.Great];
    }

    public static int[] GetJudgeCount()
    {
        return ComboManager.judgeCount.ToArray();
    }

    public static ClearMarks GetClearMark()
    {
        return RankItem.GetClearMark(ComboManager.judgeCount, GetCombo(), GetAcc());
    }

    public static Ranks GetRank()
    {
        return RankItem.GetRank(GetAcc());
    }

    public static double GetAcc()
    {
        double acc = (double)ComboManager.acc / ComboManager.maxAcc;
        return acc;
    }
}
