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

        [SecondaryKey(0, keyOrder: 0), NonUnique]
        public int ChartId { get; set; }

        [IgnoreMember]
        public V2.Difficulty Difficulty { get => (V2.Difficulty)_Difficulty; set => _Difficulty = (int)value; }

        [SecondaryKey(0, keyOrder: 1), NonUnique]
        public int _Difficulty { get; set; }

        [SecondaryKey(1), NonUnique]
        public int MusicId { get; set; }

        public int[] Judge { get; set; } = new int[(int)JudgeResult.Max + 1];

        public double Acc { get; set; }

        public int Combo { get; set; }

        public int Score { get; set; }

        public ulong Mods { get; set; }

        [SecondaryKey(2), NonUnique]
        public string ChartHash { get; set; }

        public string ReplayFile { get; set; }

        [SecondaryKey(3), NonUnique]
        public DateTime CreatedAt { get; set; }

        public static ClearMarks GetClearMark(int[] Judge, int Combo, double Acc)
        {
            int noteCount = 0;
            for (int i = 0; i <= (int)JudgeResult.Max; i++)
            {
                noteCount += Judge[i];
            }
            if (noteCount == 0)
            {
                return ClearMarks.F;
            }
            if (Judge[(int)JudgeResult.Perfect] == noteCount)
            {
                return ClearMarks.AP;
            }
            else if (Combo == ComboManager.noteCount)
            {
                return ClearMarks.FC;
            }
            else if (Acc >= 0.6)
            {
                return ClearMarks.CL;
            }
            else
            {
                return ClearMarks.F;
            }
        }

        [IgnoreMember]
        public ClearMarks ClearMark = ClearMarks.F;

        public static Ranks GetRank(double Acc)
        {
            if (Acc >= 0.998)
                return Ranks.SSS;
            else if (Acc >= 0.99)
                return Ranks.SS;
            else if (Acc >= 0.97)
                return Ranks.S;
            else if (Acc >= 0.94)
                return Ranks.A;
            else if (Acc >= 0.90)
                return Ranks.B;
            else if (Acc >= 0.85)
                return Ranks.C;
            else if (Acc >= 0.60)
                return Ranks.D;
            else
                return Ranks.F;
        }

        [IgnoreMember]
        public Ranks Rank => GetRank(Acc);
    }
}
