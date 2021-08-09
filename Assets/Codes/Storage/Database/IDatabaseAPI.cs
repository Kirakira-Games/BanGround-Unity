using BanGround.Database.Generated;
using BanGround.Database.Models;
using Difficulty = V2.Difficulty;

namespace BanGround.Database
{
    public interface IDatabaseAPI
    {
        MemoryDatabase DB { get; }

        ChartSet[] GetAllChartSets();
        RankItem GetBestRank(int sid, Difficulty difficulty);
        ChartSet GetChartSetBySid(int sid);
        ChartSet[] GetChartSetsByMid(int mid);
        RankItem[] GetRankItems(int sid, Difficulty difficulty);
        void SaveChartSet(int sid, int mid, int[] difficulties);
        void Reload();
        void RemoveChartSetDifficulty(int sid, Difficulty difficulty);
        void RemoveChartSets(int[] sids);
        void RemoveRankItem(RankItem item);
        void Save();
        void SaveRankItem(RankItem item);
    }
}
