using UnityEngine;
using System.Collections;
using UniRx.Async;
using System;

public static class ChartVersion
{
    const int VERSION = 2;

    public static bool CanRead(int version)
    {
        return version == VERSION;
    }

    public static bool CanConvert(int version)
    {
        return version <= 1;
    }

    public static V2.Chart ConvertFromV1(cHeader header, Difficulty difficulty)
    {
        Chart chart = DataLoader.LoadChart<Chart>(header.sid, difficulty);
        V2.Chart newChart = V2.Chart.From(chart);
        // DataLoader.SaveChart(newChart, header.sid, difficulty);
        return newChart;
    }

    public static async UniTask<V2.Chart> Process(cHeader header, Difficulty difficulty)
    {
        if (!CanRead(header.version))
        {
            if (!CanConvert(header.version))
            {
                return null;
            }
            if (!await BGEditor.MessageBox.ShowMessage("Unsupported chart",
                "This chart uses an unsupported standard.\nConvert? (animations and timing information will be lost)"))
            {
                return null;
            }
            return ConvertFromV1(header, difficulty);
        }
        else if (CanConvert(header.version))
        {
            if (!await BGEditor.MessageBox.ShowMessage("Outdated chart",
                "This chart uses a deprecated standard.\nBut you can still play it without conversion.\nConvert? (animations and timing information will be lost)"))
            {
                return DataLoader.LoadChart<V2.Chart>(header.sid, difficulty);
            }
            return ConvertFromV1(header, difficulty);
        }
        else
        {
            return DataLoader.LoadChart<V2.Chart>(header.sid, difficulty);
        }
    }
}
