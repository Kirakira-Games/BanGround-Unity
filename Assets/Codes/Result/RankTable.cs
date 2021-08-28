using BanGround.Database;
using BanGround.Identity;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using V2;
using Zenject;

public class RankTable : MonoBehaviour
{
    [Inject]
    IAccountManager accountManager;
    [Inject]
    private IDatabaseAPI db;
    [Inject]
    private DiContainer diContainer;

    public Transform Content;
    public GameObject RankCellPrefab;
    public Toggle localRank;
    public Toggle worldRank;

    int currentSid;
    Difficulty currentDifficulty;

    public void ClearRankCells()
    {
        for (int i = 0; i < Content.childCount; i++)
        {
            var cellObj = Content.GetChild(i).gameObject;
            Destroy(cellObj);
        }
    }

    public void ShowLocalRanks()
    {
        ClearRankCells();

        var ranks = db.GetRankItems(currentSid, currentDifficulty).OrderByDescending(r => r.Score).ToArray();
        var name = accountManager.ActiveUser.Nickname;

        for (int i = 0; i < ranks.Length; i++)
        {
            var rankCellObj = diContainer.InstantiatePrefab(RankCellPrefab, Content);
            var rankCell = rankCellObj.GetComponent<RankCell>();

            rankCell.UpdateLocalRankItem(ranks[i], i + 1, name);
        }
    }

    public void OnChangeRankSource()
    {
        if (localRank.isOn)
        {
            ShowLocalRanks();
        }
        else if (worldRank.isOn)
        {
            // TODO;
        }
    }

    public void UpdateCurrentChart(int sid, Difficulty diff)
    {
        currentSid = sid;
        currentDifficulty = diff;

        OnChangeRankSource();
    }
}
