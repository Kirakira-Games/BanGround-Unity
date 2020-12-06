using MasterMemory;
using MessagePack;
using System;

namespace BanGround.Database.Models
{
    [MemoryTable("rank"), MessagePackObject(true)]
    public class RankItem
    {
        [PrimaryKey]
        public int Id { get; set; }

        [SecondaryKey(0, keyOrder: 1), NonUnique]
        public int ChartId { get; set; }
        [SecondaryKey(0, keyOrder: 1), NonUnique]
        public Difficulty Difficulty { get; set; }

        public ClearMarks ClearMark { get; set; }

        public Ranks Rank { get; set; }

        public int[] Judge { get; set; }

        [SecondaryKey(1), NonUnique]
        public int MusicId { get; set; }

        public float Acc { get; set; }

        public int Combo { get; set; }

        public int Score { get; set; }

        public int Mods { get; set; }

        [SecondaryKey(2), NonUnique]
        public string ChartHash { get; set; }

        public string ReplayFile { get; set; }

        [SecondaryKey(3), NonUnique]
        public DateTime CreatedAt { get; set; }
    }
}