using BanGround.Database.Models;
using BanGround.Identity;
using BanGround.Scene.Params;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RankCell : MonoBehaviour
{
    public Image ClearMark;
    public Image RankMark;
    public Text Name;
    public Text Score;
    public Text Rank;
    public Text Acc;

    public Sprite[] ClearMarkIcons;
    public Sprite[] RankIcons;

    RankItem rankItem;

    [Inject]
    private IChartLoader chartLoader;

    public void Inject(IChartLoader _chartLoader)
    {
        chartLoader = _chartLoader;
    }

    public void UpdateLocalRankItem(RankItem item, int rank, string name)
    {
        rankItem = item;

        Name.text = name;
        Score.text = string.Format(ComboManager.FORMAT_DISPLAY_SCORE, rankItem.Score);
        Rank.text = $"#{rank}";
        Acc.text = $"{rankItem.Acc:P2}";

        ClearMark.sprite = ClearMarkIcons[(int)item.ClearMark];
        RankMark.sprite = RankIcons[(int)item.Rank];
    }

    public void OnClicked()
    {
        if (rankItem.ReplayFile != null)
        {
            var gameParams = SceneLoader.GetParamsOrDefault<InGameParams>();
            gameParams.difficulty = rankItem.Difficulty;
            gameParams.sid = rankItem.ChartId;
            gameParams.saveRecord = false;
            gameParams.saveReplay = false;
            gameParams.replayPath = rankItem.ReplayFile;
            gameParams.mods = (BanGround.Game.Mods.ModFlag)rankItem.Mods;
            gameParams.isOffsetGuide = false;

            SceneLoader.LoadScene(
                "InGame", 
                () => chartLoader.LoadChart(
                    rankItem.ChartId, 
                    rankItem.Difficulty, 
                    true
                ), 
                pushStack: true,
                parameters: gameParams
            );
        }
    }
}
