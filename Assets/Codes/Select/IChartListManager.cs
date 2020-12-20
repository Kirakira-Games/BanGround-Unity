using System.Collections.Generic;
using UnityEngine.Events;

public interface IChartListManager
{
    List<cHeader> chartList { get; }
    ChartIndexInfo current { get; }
    UnityEvent onChartListUpdated { get; }
    UnityEvent onDifficultyUpdated { get; }
    UnityEvent onSelectedChartUpdated { get; }

    void SelectChartByIndex(int index);
    void SelectChartBySid(int sid);
    void SelectDifficulty(Difficulty difficulty);
    void SortChart();
}