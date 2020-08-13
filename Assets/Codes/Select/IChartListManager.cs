using System.Collections.Generic;
using UniRx.Async;
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

    void ClearForcedChart();
    void ForceChart(int sid, Difficulty difficulty);
    void SelectChartByIndex(int index);
    void SelectChartBySid(int sid);
    void SelectDifficulty(Difficulty difficulty);
    void SortChart();
    UniTask<bool> LoadChart(bool convertToGameChart);
}