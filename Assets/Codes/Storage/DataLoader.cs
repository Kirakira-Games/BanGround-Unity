using UnityEngine;
using System.Collections;

public class DataLoader
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static readonly string DataDir = Application.persistentDataPath + "/data/";
#else 
    public static readonly string DataDir = Application.streamingAssetsPath + "/data/";
#endif
    public static readonly string ChartDir = DataDir + "chart/";
    public static readonly string MusicDir = DataDir + "music/";

    public const int ChartVersion = 1;
}
