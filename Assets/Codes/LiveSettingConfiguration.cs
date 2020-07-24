using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UniRx.Async;
using System;

public static class LiveSetting
{
    private static DemoFile _demoFile = null;

    public static DemoFile DemoFile
    {
        get
        {
            var demo = _demoFile;
            _demoFile = null;
            return demo;
        }
        set
        {
            _demoFile = value;
        }
    }

    static KVar fs_assetpath = new KVar("fs_assetpath", "V2Assets", KVarFlags.Hidden | KVarFlags.StringOnly);
    static KVar fs_iconpath = new KVar("fs_iconpath", "UI/ClearMark/", KVarFlags.Hidden | KVarFlags.StringOnly);
    static KVar cl_lastdiff = new KVar("cl_lastdiff", "0", KVarFlags.Archive, "Current chart set difficulty", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    public static string assetDirectory => fs_assetpath;
    public static string IconPath => fs_iconpath;
    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public static int currentChart = 0;
    public static KVarRef currentDifficulty = new KVarRef("cl_lastdiff");
    private static int cachedActualDifficulty = currentDifficulty;
    public static int actualDifficulty
    {
        get
        {
            return offsetAdjustMode ? (int)Difficulty.Easy : cachedActualDifficulty;
        }
        set
        {
            cachedActualDifficulty = value;
        }
    } // actualDifficulty may differ if a chart set does not have currentDifficulty

    public const int offsetAdjustChart = 99901;
    public static cHeader CurrentHeader
    {
        get
        {
            if (offsetAdjustMode)
            {
                return DataLoader.chartList.Where((x) => x.sid == offsetAdjustChart).First();
            }
            else
            {
                return DataLoader.chartList[currentChart];
            }
        }
    }
    public static V2.Chart chart;
    public static GameChartData gameChart;

    public static readonly string settingsPath = Application.persistentDataPath + "/LiveSettings.json";
    public static readonly string scoresPath = Application.persistentDataPath + "/Scores.bin";

    public static List<ModBase> attachedMods = new List<ModBase>();
    public static float SpeedCompensationSum = 1.0f;
    public static bool offsetAdjustMode = false;

    //public static bool fullScreen;

    public static bool AddMod(ModBase mod)
    {
        if (mod == null) return false;
        if (!attachedMods.Contains(mod))
        {
            if (attachedMods.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(mod))))
                return false;

            attachedMods.Add(mod);
            if (mod is AudioMod)
            {
                SpeedCompensationSum *= (mod as AudioMod).SpeedCompensation;
            }
        }

        return true;
    }

    public static void RemoveMod(ModBase mod)
    {
        if (attachedMods.Contains(mod))
        {
            attachedMods.Remove(mod);

            if (mod is AudioMod)
            {
                SpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public static void RemoveAllMods()
    {
        foreach (var mod in attachedMods)
        {
            if (mod is AudioMod)
            {
                SpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public static async UniTask<bool> LoadChart(bool convertToGameChart)
    {
        chart = await ChartVersion.Process(CurrentHeader, (Difficulty)actualDifficulty);
        if (chart == null)
        {
            MessageBannerController.ShowMsg(LogLevel.ERROR, "This chart is unsupported.");
            return false;
        }
        try
        {
            if (convertToGameChart)
            {
                gameChart = ChartLoader.LoadChart(
                    JsonConvert.DeserializeObject<V2.Chart>(
                        JsonConvert.SerializeObject(chart)
                    ));
            }
            return true;
        }
        catch (Exception e)
        {
            MessageBannerController.ShowMsg(LogLevel.ERROR, e.Message);
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    static KVarRef r_notespeed = new KVarRef("r_notespeed");

    public static int NoteScreenTime
    {
        get
        {
            if (cachedSpeed != r_notespeed)
            {
                cachedSpeed = r_notespeed;
                cachedScreenTime = (int)(-540f * r_notespeed + 6500);
            }

            return (int)(cachedScreenTime * SpeedCompensationSum);
        }
    }
}

public enum NoteStyle
{
    Circle,
    Cube,
    Dark,
    Custom
}

public enum SEStyle
{
    None,
    Drum,
    Bbben,
    AECBanana,
    Custom
}