using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayRecordDisplay : MonoBehaviour
{
    [Inject]
    private IChartListManager chartListManager;

    [Inject(Id = "fs_iconpath")]
    private KVar fs_iconpath;

    RawImage Rank;
    RawImage clearMark;
    Text score;
    Text acc;
    public static PlayRecords playRecords;
    private void Awake()
    {
        //Marks
        Rank = GameObject.Find("Rank").GetComponent<RawImage>();
        clearMark = GameObject.Find("ClearMark").GetComponent<RawImage>();
        score = GameObject.Find("ScoreHistory").GetComponent<Text>();
        acc = GameObject.Find("AccText").GetComponent<Text>();
        playRecords = PlayRecords.OpenRecord();
    }
    public void DisplayRecord()
    {
        int count = 0;
        PlayResult a = new PlayResult();
        for (int i = 0; i < playRecords.resultsList.Count; i++)
        {
            if (playRecords.resultsList[i].ChartId == chartListManager.current.header.sid &&
                playRecords.resultsList[i].Difficulty == chartListManager.current.difficulty)
            {
                count++;
                a = playRecords.resultsList[i];

            }
        }
        score.text = string.Format("{0:0000000}", a.Score);
        acc.text = string.Format("{0:P2}", Mathf.FloorToInt((float)a.Acc * 10000) / 10000f);
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


        var rank = new Texture2D(0, 0);
        switch (a.ranks)
        {
            case Ranks.SSS:
                rank = Resources.Load(fs_iconpath + "SSS") as Texture2D;
                break;
            case Ranks.SS:
                rank = Resources.Load(fs_iconpath + "SS") as Texture2D;
                break;
            case Ranks.S:
                rank = Resources.Load(fs_iconpath + "S") as Texture2D;
                break;
            case Ranks.A:
                rank = Resources.Load(fs_iconpath + "A") as Texture2D;
                break;
            case Ranks.B:
                rank = Resources.Load(fs_iconpath + "B") as Texture2D;
                break;
            case Ranks.C:
                rank = Resources.Load(fs_iconpath + "C") as Texture2D;
                break;
            case Ranks.D:
                rank = Resources.Load(fs_iconpath + "D") as Texture2D;
                break;
            case Ranks.F:
                rank = Resources.Load(fs_iconpath + "F") as Texture2D;
                break;
            default:
                rank = null;
                break;
        }
        Rank.texture = rank;

        //Set Mark
        var mark = new Texture2D(0, 0);
        switch (a.clearMark)
        {
            case ClearMarks.AP:
                mark = Resources.Load(fs_iconpath + "AP") as Texture2D;
                break;
            case ClearMarks.FC:
                mark = Resources.Load(fs_iconpath + "FC") as Texture2D;
                break;
            case ClearMarks.CL:
                mark = Resources.Load(fs_iconpath + "CL") as Texture2D;
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
