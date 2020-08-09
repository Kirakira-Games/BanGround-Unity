using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UniRx.Async;
using System;
using Zenject;

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

    static KVarRef fs_assetpath = new KVarRef("fs_assetpath");
    static KVarRef fs_iconpath = new KVarRef("fs_iconpath");
    static KVarRef cl_lastdiff = new KVarRef("cl_lastdiff");

    public static string assetDirectory => fs_assetpath;
    public static string IconPath => fs_iconpath;
    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public static int currentChart = 0;
    /// <summary>
    /// 有可能不存在！获取当前谱面难度，使用<see cref="actualDifficulty"/>。
    /// 例如：当用户从EX难度切换到一个只有NM难度的谱面集，
    /// currentDifficulty为Expert，而actualDifficulty为Normal。
    /// </summary>
    public static KVarRef currentDifficulty = new KVarRef("cl_lastdiff");
    private static int cachedActualDifficulty = currentDifficulty;
    /// <summary>
    /// 获取当前谱面难度。一定是当前谱面集拥有的难度，但不一定是用户偏好的谱面难度。
    /// 例如：当用户从EX难度切换到一个只有NM难度的谱面集，
    /// currentDifficulty为Expert，而actualDifficulty为Normal。
    /// </summary>
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
    }

    public const int offsetAdjustChart = 99901;
    public static cHeader CurrentHeader
    {
        get
        {
            if (offsetAdjustMode)
            {
                var ret = DataLoader.Instance.chartList.Where((x) => x.sid == offsetAdjustChart).First();
                ret.LoadDifficultyLevels(DataLoader.Instance);
                return ret;
            }
            else
            {
                return DataLoader.Instance.chartList[currentChart];
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
        chart = await ChartVersion.Instance.Process(CurrentHeader, (Difficulty)actualDifficulty);
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