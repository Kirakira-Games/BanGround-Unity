using UnityEngine;
using System.Collections;
using UniRx.Async;

public static class ChartVersion
{
    const int VERSION = 2;

    public static bool CanRead(int version)
    {
        return true;
        //return version == VERSION;
    }

    public static bool CanConvert(int version)
    {
        return true;
        //return version == 1;
    }

    public static async UniTask<bool> Process(cHeader header, Chart _)
    {
        if (!CanRead(header.version))
        {
            if (!CanConvert(header.version))
            {
                return false;
            }
            if (!await BGEditor.MessageBox.ShowMessage("Unsupported chart",
                "This chart uses an unsupported standard.\nConvert? (animations and timing information will be lost)"))
            {
                return false;
            }
        }
        else if (CanConvert(header.version))
        {
            if (!await BGEditor.MessageBox.ShowMessage("Outdated chart",
                "This chart uses a deprecated standard.\nBut you can still play it without conversion.\nConvert? (animations and timing information will be lost)"))
            {
                return true;
            }
        }
        else
        {
            return true;
        }
        // Convert chart
        MessageBoxController.ShowMsg(LogLevel.INFO, "I'm kidding, actually I cannot convert.");
        return false;
    }
}
