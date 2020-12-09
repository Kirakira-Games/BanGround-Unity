using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

public interface IChartListManager
{
    List<cHeader> chartList { get; }
    ChartIndexInfo current { get; }
    UnityEvent onChartListUpdated { get; }
    UnityEvent onDifficultyUpdated { get; }
    UnityEvent onSelectedChartUpdated { get; }
    V2.Chart chart { get; }
    GameChartData gameChart { get; }
    bool offsetAdjustMode { get; }
    int offsetAdjustSid { get; }
    Difficulty offsetAdjustDiff { get; }

    void ClearForcedChart();
    void ForceChart(int sid, Difficulty difficulty);
    void ForceOffsetChart();
    void SelectChartByIndex(int index);
    void SelectChartBySid(int sid);
    void SelectDifficulty(Difficulty difficulty);
    void SortChart();
    Dictionary<string, byte[]> ComputeCurrentChartHash();
    UniTask<bool> LoadChart(bool convertToGameChart);
}