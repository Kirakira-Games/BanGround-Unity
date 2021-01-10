using MasterMemory;
using MessagePack;

namespace BanGround.Database.Models
{
    [MemoryTable("chart"), MessagePackObject(true)]
    public class ChartSet
    {
        [PrimaryKey]
        public int Sid { get; set; }

        [SecondaryKey(0), NonUnique]
        public int Mid { get; set; }

        public int[] Difficulties { get; set; } = new int[(int)Difficulty.Special + 1];
    }
}