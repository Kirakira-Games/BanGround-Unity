using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChartLoader
{
    public static Header LoadHeaderFromFile(string path)
    {
        TextAsset headerText = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<Header>(headerText.text);
    }
    public static Chart LoadChartFromFile(string path)
    {
        TextAsset chartText = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<Chart>(chartText.text);
    }
}
