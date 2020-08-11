using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayRecordDisplay : MonoBehaviour
{
    [Inject]
    private ILiveSetting liveSetting;

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
            if (playRecords.resultsList[i].ChartId == liveSetting.CurrentHeader.sid &&
                playRecords.resultsList[i].Difficulty == (Difficulty)liveSetting.actualDifficulty)
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
                rank = Resources.Load(liveSetting.IconPath + "SSS") as Texture2D;
                break;
            case Ranks.SS:
                rank = Resources.Load(liveSetting.IconPath + "SS") as Texture2D;
                break;
            case Ranks.S:
                rank = Resources.Load(liveSetting.IconPath + "S") as Texture2D;
                break;
            case Ranks.A:
                rank = Resources.Load(liveSetting.IconPath + "A") as Texture2D;
                break;
            case Ranks.B:
                rank = Resources.Load(liveSetting.IconPath + "B") as Texture2D;
                break;
            case Ranks.C:
                rank = Resources.Load(liveSetting.IconPath + "C") as Texture2D;
                break;
            case Ranks.D:
                rank = Resources.Load(liveSetting.IconPath + "D") as Texture2D;
                break;
            case Ranks.F:
                rank = Resources.Load(liveSetting.IconPath + "F") as Texture2D;
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
                mark = Resources.Load(liveSetting.IconPath + "AP") as Texture2D;
                break;
            case ClearMarks.FC:
                mark = Resources.Load(liveSetting.IconPath + "FC") as Texture2D;
                break;
            case ClearMarks.CL:
                mark = Resources.Load(liveSetting.IconPath + "CL") as Texture2D;
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
