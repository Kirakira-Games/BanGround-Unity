using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ScanChartsToJson : MonoBehaviour
{
    public static DataLoader dataLoader = new DataLoader();

    [MenuItem("BanGround/扫描json谱面并转为bin")]
    public static void ConvertJson2Bin()
    {
        DirectoryInfo ChartDir = new DirectoryInfo(DataLoader.ChartDir);
        DirectoryInfo[] charts = ChartDir.GetDirectories();
        foreach (DirectoryInfo chart in charts)
        {
            dataLoader.ConvertJsonToBin(chart);
        }

        DirectoryInfo MusicDir = new DirectoryInfo(DataLoader.MusicDir);
        DirectoryInfo[] songs = MusicDir.GetDirectories();
        foreach (DirectoryInfo song in songs)
        {
            dataLoader.ConvertJsonToBin(song);
        }
    }
}

