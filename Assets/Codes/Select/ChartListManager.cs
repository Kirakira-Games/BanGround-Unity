using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;

public class ChartIndexInfo
{
    public int index;
    public cHeader header;
    public Difficulty difficulty;
}

public class ChartListManager
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public List<cHeader> chartList => dataLoader.chartList;

    public UnityEvent onDifficultyUpdated { get; } = new UnityEvent();
    public UnityEvent onChartListUpdated { get; } = new UnityEvent();
    public UnityEvent onSelectedChartUpdated { get; } = new UnityEvent();

    /// <summary>
    /// This difficulty might be different from <see cref="current"/>,
    /// which is more reliable for actually selected difficulty.
    /// </summary>
    public Difficulty currentDifficulty { get; private set; }

    /// <summary>
    /// Current chart, either <see cref="selectedChart"/> or <see cref="forceChart"/>
    /// </summary>
    public ChartIndexInfo current => forceChart.header == null ? selectedChart : forceChart;
    // Currently selected chart.
    private ChartIndexInfo selectedChart = new ChartIndexInfo();
    // Force to play this chart. Used in offset adjust mode.
    private ChartIndexInfo forceChart = new ChartIndexInfo();

    private void UpdateActualDifficulty()
    {
        var header = selectedChart.header;
        int difficulty = cl_lastdiff.Get<int>();
        while (header.difficultyLevel[difficulty] == -1)
        {
            difficulty = (difficulty + 1) % ((int)Difficulty.Special + 1);
        }
        selectedChart.difficulty = (Difficulty) difficulty;
        onDifficultyUpdated.Invoke();
    }

    public void SelectChartByIndex(int index)
    {
        index = Mathf.Clamp(index, 0, chartList.Count - 1);
        selectedChart.index = index;
        selectedChart.header = chartList[index];
        cl_lastsid.Set(selectedChart.header.sid);
        UpdateActualDifficulty();
        onSelectedChartUpdated.Invoke();
    }

    public void SelectChartBySid(int sid)
    {
        int index = chartList.FindIndex((header) => header.sid == sid);
        if (index != -1)
        {
            SelectChartByIndex(index);
        }
    }

    public void SelectDifficulty(Difficulty difficulty)
    {
        cl_lastdiff.Set((int)difficulty);
        UpdateActualDifficulty();
    }

    public void ClearForcedChart()
    {
        forceChart.header = null;
    }

    public void ForceChart(int sid, Difficulty difficulty)
    {
        int index = chartList.FindIndex((header) => header.sid == sid);

        if (index != -1)
        {
            forceChart.index = index;
            forceChart.header = chartList[index];
            forceChart.difficulty = difficulty;
        }
    }

    public void SortChart(IComparer<cHeader> comparer)
    {
        chartList.Sort(comparer);
        SelectChartBySid(selectedChart.header.sid);
        onChartListUpdated.Invoke();
    }
}
