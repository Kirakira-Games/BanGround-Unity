using System.Collections.Generic;
using UnityEngine.Events;
using V2;

public interface IChartListManager
{
    List<cHeader> chartList { get; }
    ChartIndexInfo current { get; }
    UnityEvent onChartListUpdated { get; }
    UnityEvent onDifficultyUpdated { get; }
    UnityEvent onSelectedChartUpdated { get; }

    void SelectChartByIndex(int index);
    void SelectChartBySid(int sid);
    void SelectDifficulty(V2.Difficulty difficulty);
    void SortChart();
}
