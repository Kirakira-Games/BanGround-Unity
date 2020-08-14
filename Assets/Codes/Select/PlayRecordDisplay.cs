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

        Rank.texture = resourceLoader.LoadIconResource<Texture2D>(a.ranks.ToString());

        //Set Mark
        var mark = new Texture2D(0, 0);
        switch (a.clearMark)
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
