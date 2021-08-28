using BanGround;
using BanGround.Database.Models;
using BanGround.Scene.Params;
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

    private RankItem rankItem;

    [Inject]
    private IChartLoader chartLoader;
    [Inject]
    private IFileSystem fs;

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
        if (rankItem.ReplayFile != null && fs.FileExists(rankItem.ReplayFile))
        {
            var gameParams = new InGameParams
            {
                difficulty = rankItem.Difficulty,
                sid = rankItem.ChartId,
                saveRecord = false,
                saveReplay = false,
                replayPath = rankItem.ReplayFile,
                mods = (BanGround.Game.Mods.ModFlag)rankItem.Mods,
                isOffsetGuide = false
            };

            // TODO: Do not push stack when loading from result scene
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
