using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Linq;

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

    public int offsetAdjustSid { get; private set; } = 99901;
    public Difficulty offsetAdjustDiff { get; private set; } = Difficulty.Easy;
    public bool offsetAdjustMode { get; private set; } = false;

    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;
    [Inject]
    private IMessageBannerController messageBannerController;

    public List<cHeader> chartList => dataLoader.chartList;

    public UnityEvent onDifficultyUpdated { get; } = new UnityEvent();
    public UnityEvent onChartListUpdated { get; } = new UnityEvent();
    public UnityEvent onSelectedChartUpdated { get; } = new UnityEvent();

    /// <summary>
    /// Current chart, either <see cref="selectedChart"/> or <see cref="forceChart"/>
    /// </summary>
    public ChartIndexInfo current => forceChart.header == null ? selectedChart : forceChart;

    public V2.Chart chart { get; private set; }
    public GameChartData gameChart { get; private set; }

    // Currently selected chart.
    private ChartIndexInfo selectedChart = new ChartIndexInfo();
    // Force to play this chart. Used in offset adjust mode.
    private ChartIndexInfo forceChart = new ChartIndexInfo();

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

    public void ClearForcedChart()
    {
        forceChart.header = null;
        offsetAdjustMode = false;
    }

    public void ForceChart(int sid, Difficulty difficulty)
    {
        ClearForcedChart();

        int index = chartList.FindIndex((header) => header.sid == sid);

        if (index != -1)
        {
            forceChart.index = index;
            forceChart.header = chartList[index];
            forceChart.header.LoadDifficultyLevels(dataLoader);
            forceChart.difficulty = difficulty;
        }
    }

    public void ForceOffsetChart()
    {
        ForceChart(offsetAdjustSid, offsetAdjustDiff);
        offsetAdjustMode = true;
    }

    public void SortChart()
    {
        chartList.Sort(sorterFactory.Create());
        SelectChartBySid(cl_lastsid);
        onChartListUpdated.Invoke();
    }

    [Inject(Id = "r_mirror")]
    KVar r_mirror;

    public async UniTask<bool> LoadChart(bool convertToGameChart)
    {
        chart = await ChartVersion.Instance.Process(current.header, current.difficulty);
        if (chart == null)
        {
            messageBannerController.ShowMsg(LogLevel.ERROR, "This chart is unsupported.");
            return false;
        }
        try
        {
            if (convertToGameChart)
            {
                gameChart = LoadChartInternal(
                        JsonConvert.DeserializeObject<V2.Chart>(
                        JsonConvert.SerializeObject(chart)
                    ));
            }
            return true;
        }
        catch (Exception e)
        {
            messageBannerController.ShowMsg(LogLevel.ERROR, e.Message);
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    private GameChartData LoadChartInternal(V2.Chart chart)
    {
        ChartLoader.numNotes = 0;
        var timing = new ChartTiming(chart.bpm, chart.offset, ModManager.Instance.NoteScreenTime, r_mirror);
        List<GameNoteData> gameNotes = new List<GameNoteData>();
        for (int i = 0; i < chart.groups.Count; i++)
        {
            ChartLoader.LoadTimingGroup(timing, i, chart.groups[i]).ForEach(note => gameNotes.Add(note));
        }

        // Sort notes by animation order
        gameNotes.Sort(new GameNoteComparer());

        return new GameChartData
        {
            isFuwafuwa = ChartLoader.IsChartFuwafuwa(gameNotes),
            numNotes = ChartLoader.numNotes,
            notes = gameNotes.ToArray(),
            groups = chart.groups.Select(x => ChartLoader.ToGameTimingGroup(x)).ToArray(),
            bpm = chart.bpm.ToArray()
        };
    }
}
