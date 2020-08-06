using UnityEngine;
using System.Collections;
using UniRx.Async;
using System;
using Newtonsoft.Json;
using Zenject;

public class ChartVersion : IChartVersion
{
    public static IChartVersion Instance; // Temporary singleton for refactoring. TODO: Remove.
    [Inject]
    private IDataLoader dataLoader;

    const int VERSION = 2;

    public bool CanRead(int version)
    {
        return version == VERSION;
    }

    public bool CanConvert(int version)
    {
        return version <= 1;
    }

    public V2.Chart ConvertFromV1(cHeader header, Difficulty difficulty)
    {
        var chart = dataLoader.LoadChart<Chart>(header.sid, difficulty);
        V2.Chart newChart = V2.Chart.From(chart);
        dataLoader.SaveChart(newChart, header.sid, difficulty);
        return newChart;
    }

    public async UniTask<V2.Chart> Process(cHeader header, Difficulty difficulty)
    {
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
            if (!await BGEditor.MessageBox.ShowMessage("Unsupported chart",
                "This chart uses an unsupported standard.\nConvert? (animations and speed information will be lost)"))
            {
                return null;
            }
            return ConvertFromV1(header, difficulty);
        }
        else if (CanConvert(chart.version))
        {
            if (!await BGEditor.MessageBox.ShowMessage("Outdated chart",
                "This chart uses a deprecated standard.\nBut you can still play it without conversion.\nConvert? (animations and speed information will be lost)"))
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
