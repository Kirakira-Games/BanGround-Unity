using BanGround.Database;
using BanGround.Database.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayRecordDisplay : MonoBehaviour
{
    [Inject]
    private IChartListManager chartListManager;

    [Inject]
    private IResourceLoader resourceLoader;

    [Inject]
    private IDatabaseAPI db;

    private RawImage Rank;
    private RawImage clearMark;
    private Text score;
    private Text acc;

    private void Awake()
    {
        //Marks
        Rank = GameObject.Find("Rank").GetComponent<RawImage>();
        clearMark = GameObject.Find("ClearMark").GetComponent<RawImage>();
        score = GameObject.Find("ScoreHistory").GetComponent<Text>();
        acc = GameObject.Find("AccText").GetComponent<Text>();
    }
    public void DisplayRecord()
    {
        int count = 0;
        var rank = db.GetBestRank(chartListManager.current.header.sid, chartListManager.current.difficulty) ?? new RankItem();
        score.text = string.Format(ComboManager.FORMAT_DISPLAY_SCORE, rank.Score);
        acc.text = string.Format("{0:P2}", rank.Acc);
        //Set Rank
        if (count == 0)
        {
            Rank.enabled = false;
            clearMark.enabled = false;
            return;
        }
        else
        {
            Rank.enabled = true;
            clearMark.enabled = true;
        }

        Rank.texture = resourceLoader.LoadIconResource<Texture2D>(rank.Rank.ToString());

        //Set Mark
        var mark = new Texture2D(0, 0);
        switch (rank.ClearMark)
        {
            case ClearMarks.AP:
                mark = resourceLoader.LoadIconResource<Texture2D>("AP") as Texture2D;
                break;
            case ClearMarks.FC:
                mark = resourceLoader.LoadIconResource<Texture2D>("FC") as Texture2D;
                break;
            case ClearMarks.CL:
                mark = resourceLoader.LoadIconResource<Texture2D>("CL") as Texture2D;
                break;
            case ClearMarks.F:
                clearMark.enabled = false;
                break;
            default:
                clearMark.enabled = false;
                break;
        }
        clearMark.texture = mark;
    }
}
