using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.Events;
using V2;

public class ChartIndexInfo
{
    public int index;
    public cHeader header;
    public Difficulty difficulty;
}

public class ChartListManager : IChartListManager
{
    private static readonly List<cHeader> PLACEHOLDER_LIST = new List<cHeader>()
    {
        new cHeader
        {
            mid = -1,
            sid = 0,
            author = "meigong",
            authorNick = "Rino",
            difficultyLevel = new List<int>() {1, 2, 3, 4, 5},
            preview = new float[]{0, 1}
        }
    };

    private IDataLoader dataLoader;
    private ISorterFactory sorterFactory;

    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public List<cHeader> chartList {
        get
        {
            if (dataLoader.chartList != null && dataLoader.chartList.Count > 0)
                return dataLoader.chartList;
            return PLACEHOLDER_LIST;
        }
    }

    public UnityEvent onDifficultyUpdated { get; } = new UnityEvent();
    public UnityEvent onChartListUpdated { get; } = new UnityEvent();
    public UnityEvent onSelectedChartUpdated { get; } = new UnityEvent();

    /// <summary>
    /// Current chart.
    /// </summary>
    public ChartIndexInfo current => selectedChart;


    // Currently selected chart.
    private ChartIndexInfo selectedChart = new ChartIndexInfo {
        header = PLACEHOLDER_LIST[0],
        difficulty = Difficulty.Easy,
        index = 0
    };

    public ChartListManager(IDataLoader dataLoader, ISorterFactory sorterFactory)
    {
        this.dataLoader = dataLoader;
        this.sorterFactory = sorterFactory;
        dataLoader.onSongListRefreshed.AddListener(() =>
        {
            SortChart();
            SelectDifficulty((Difficulty)cl_lastdiff);
        });
    }

    private void UpdateActualDifficulty()
    {
        var header = selectedChart.header;
        header.LoadDifficultyLevels(dataLoader);

        int difficulty = cl_lastdiff;
        int circleshit = 0;

        while (header.difficultyLevel[difficulty] == -1 && ++circleshit < 6)
        {
            difficulty = (difficulty + 1) % ((int)Difficulty.Special + 1);
        }

        if (selectedChart.difficulty == (Difficulty)difficulty)
            return;
        selectedChart.difficulty = (Difficulty)difficulty;
        onDifficultyUpdated.Invoke();
    }

    public void SelectChartByIndex(int index)
    {
        if (chartList.Count == 0)
            return;
        index = Mathf.Clamp(index, 0, chartList.Count - 1);
        if (ReferenceEquals(selectedChart.header, chartList[index]) && selectedChart.index == index)
        {
            return;
        }
        selectedChart.index = index;
        selectedChart.header = chartList[index];
        selectedChart.header.LoadDifficultyLevels(dataLoader);
        cl_lastsid.Set(selectedChart.header.sid);
        UpdateActualDifficulty();
        onSelectedChartUpdated.Invoke();
    }

    public void SelectChartBySid(int sid)
    {
        int index = chartList.FindIndex((header) => header.sid == sid);
        if (index == -1)
        {
            index = Mathf.Min(chartList.Count - 1, current.index);
        }
        SelectChartByIndex(index);
    }

    public void SelectDifficulty(Difficulty difficulty)
    {
        cl_lastdiff.Set((int)difficulty);
        UpdateActualDifficulty();
    }

    public void SortChart()
    {
        chartList.Sort(sorterFactory.Create());
        SelectChartBySid(cl_lastsid);
        onChartListUpdated.Invoke();
    }
}
