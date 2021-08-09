using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using V2;

public interface IChartLoader
{
    cHeader header { get; }
    V2.Chart chart { get; }
    GameChartData gameChart { get; }

    UniTask<bool> LoadChart(int sid, Difficulty difficulty, bool convertToGameChart);
    Dictionary<string, byte[]> GetChartHash(int mid, int sid, Difficulty difficulty);
}
