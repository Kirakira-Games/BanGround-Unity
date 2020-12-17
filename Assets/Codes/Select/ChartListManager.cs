using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Linq;
using BanGround;
using System.Security.Cryptography;
using System.IO;
using BanGround.Game.Mods;
using BanGround.Scene.Params;

public class ChartIndexInfo
{
    public int index;
    public cHeader header;
    public Difficulty difficulty;
}

public class ChartListManager : IChartListManager
{
    private IDataLoader dataLoader;
    private ISorterFactory sorterFactory;

    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public List<cHeader> chartList => dataLoader.chartList;

    public UnityEvent onDifficultyUpdated { get; } = new UnityEvent();
    public UnityEvent onChartListUpdated { get; } = new UnityEvent();
    public UnityEvent onSelectedChartUpdated { get; } = new UnityEvent();

    /// <summary>
    /// Current chart.
    /// </summary>
    public ChartIndexInfo current => selectedChart;


    // Currently selected chart.
    private ChartIndexInfo selectedChart = new ChartIndexInfo();

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
        if (index != -1)
        {
            SelectChartByIndex(index);
        }
        else
        {
            SelectChartByIndex(chartList.Count - 1);
        }
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
