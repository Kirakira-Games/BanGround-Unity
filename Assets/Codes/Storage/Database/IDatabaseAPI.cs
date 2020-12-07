using BanGround.Database.Generated;
using BanGround.Database.Models;

namespace BanGround.Database
{
    public interface IDatabaseAPI
    {
        MemoryDatabase DB { get; }

        RankItem[] GetRankItems(int sid, Difficulty difficulty);
        RankItem GetBestRank(int sid, Difficulty difficulty);
        void SaveRankItem(RankItem item);
        void Reload();
        void RemoveRankItem(RankItem item);
        void Save();
    }
}