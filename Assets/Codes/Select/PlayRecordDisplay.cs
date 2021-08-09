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

    public RawImage Rank;
    public RawImage ClearMark;
    public Text Score;
    public Text Acc;

    public void DisplayRecord()
    {
        var rank = db.GetBestRank(chartListManager.current.header.sid, chartListManager.current.difficulty);

        // Set rank and clearmark display
        Rank.enabled = ClearMark.enabled = rank != null;
        if (rank == null)
            rank = new RankItem();

        // Set score and acc
        Score.text = string.Format(ComboManager.FORMAT_DISPLAY_SCORE, rank.Score);
        Acc.text = string.Format("{0:P2}", rank.Acc);

        Rank.texture = resourceLoader.LoadIconResource<Texture2D>(rank.Rank.ToString());

        // Set Mark
        var mark = new Texture2D(0, 0);
        switch (rank.ClearMark)
        {
            case ClearMarks.AP:
                mark = resourceLoader.LoadIconResource<Texture2D>("AP");
                break;
            case ClearMarks.FC:
                mark = resourceLoader.LoadIconResource<Texture2D>("FC");
                break;
            case ClearMarks.CL:
                mark = resourceLoader.LoadIconResource<Texture2D>("CL");
                break;
            case ClearMarks.F:
                ClearMark.enabled = false;
                break;
            default:
                ClearMark.enabled = false;
                break;
        }
        ClearMark.texture = mark;
    }
}
