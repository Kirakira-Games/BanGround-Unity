﻿using Cysharp.Threading.Tasks;
using Zenject;
using V2;

public class ChartVersion : IChartVersion
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IMessageBox messageBox;

    public const int VERSION = 2;

    public bool CanRead(int version)
    {
        return version == VERSION;
    }

    public bool CanConvert(int version)
    {
        return version <= 1;
    }

    public V2.Chart ConvertFromV1(cHeader header, V2.Difficulty difficulty)
    {
        var chart = dataLoader.LoadChart<Chart>(header.sid, difficulty);
        V2.Chart newChart = V2.Chart.From(chart);
        dataLoader.SaveChart(newChart, header.sid, difficulty);
        return newChart;
    }

    public async UniTask<V2.Chart> Process(cHeader header, V2.Difficulty difficulty)
    {
        if (header == null)
            return null;

        V2.Chart chart = new V2.Chart();
        try
        {
            chart = dataLoader.LoadChart<V2.Chart>(header.sid, difficulty);
        }
        catch
        {
            chart.version = 0;
        }
        if (!CanRead(chart.version))
        {
            if (!CanConvert(chart.version))
            {
                return null;
            }
            if (!await messageBox.ShowMessage("ChartVersion.Title.ConvertUnsupported".L(), "ChartVersion.Prompt.ConvertUnsupported".L()))
            {
                return null;
            }
            return ConvertFromV1(header, difficulty);
        }
        else if (CanConvert(chart.version))
        {
            if (!await messageBox.ShowMessage("ChartVersion.Title.ConvertOutdated".L(), "ChartVersion.Prompt.ConvertOutdated".L()))
            {
                return dataLoader.LoadChart<V2.Chart>(header.sid, difficulty);
            }
            return ConvertFromV1(header, difficulty);
        }
        else
        {
            return chart;
        }
    }
}
