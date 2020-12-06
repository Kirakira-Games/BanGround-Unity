using MasterMemory;
using MessagePack;

namespace BanGround.Database.RawModels
{
    [MemoryTable("Rank"), MessagePackObject(true)]
    public class RankItem
    {
        [PrimaryKey]
        public int Id { get; set; }

        [SecondaryKey(0), NonUnique]
        [SecondaryKey(1, keyOrder: 1), NonUnique]
        public int ChartId { get; set; }
        [SecondaryKey(1, keyOrder: 1), NonUnique]
        public Difficulty difficulty { get; set; }

        [SecondaryKey(2), NonUnique]
        public int MusicId { get; set; }

        public float Acc { get; set; }

        public Ranks Rank { get; set; }

        [SecondaryKey(3), NonUnique]
        public string ChartHash { get; set; }

        public string ReplayFile { get; set; }
    }
}