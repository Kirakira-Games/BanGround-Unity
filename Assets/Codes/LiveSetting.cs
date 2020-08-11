using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UniRx.Async;
using System;
using Zenject;

public class LiveSetting : ILiveSetting
{
    public static ILiveSetting Instance;

    [Inject]
    private IDataLoader dataLoader;
    [Inject(Id = "fs_assetpath")]
    private KVar fs_assetpath;
    [Inject(Id = "fs_iconpath")]
    private KVar fs_iconpath;
    [Inject(Id = "r_notespeed")]
    private KVar r_notespeed;

    private DemoFile _demoFile = null;

    public DemoFile DemoFile
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

    public string assetDirectory => fs_assetpath;
    public string IconPath => fs_iconpath;

    public int currentChart { get; set; } = 0;
    private int cachedActualDifficulty;

    public LiveSetting([Inject(Id = "cl_lastdiff")] KVar cl_lastdiff)
    {
        cachedActualDifficulty = cl_lastdiff;
    }
    /// <summary>
    /// 获取当前谱面难度。一定是当前谱面集拥有的难度，但不一定是用户偏好的谱面难度。
    /// 例如：当用户从EX难度切换到一个只有NM难度的谱面集，
    /// currentDifficulty为Expert，而actualDifficulty为Normal。
    /// </summary>
    public int actualDifficulty
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
    public cHeader CurrentHeader
    {
        get
        {
            if (offsetAdjustMode)
            {
                var ret = dataLoader.chartList.Where((x) => x.sid == offsetAdjustChart).First();
                ret.LoadDifficultyLevels(dataLoader);
                return ret;
            }
            else
            {
                return dataLoader.chartList[currentChart];
            }
        }
    }
    public V2.Chart chart { get; private set; }
    public GameChartData gameChart { get; private set; }

    public static readonly string settingsPath = Application.persistentDataPath + "/LiveSettings.json";
    public static readonly string scoresPath = Application.persistentDataPath + "/Scores.bin";

    public List<ModBase> attachedMods { get; set; } = new List<ModBase>();
    public float SpeedCompensationSum { get; set; } = 1.0f;
    public bool offsetAdjustMode { get; set; } = false;

    //public static bool fullScreen;

    public bool AddMod(ModBase mod)
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

    public void RemoveMod(ModBase mod)
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

    public void RemoveAllMods()
    {
        foreach (var mod in attachedMods)
        {
            if (mod is AudioMod)
            {
                SpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public async UniTask<bool> LoadChart(bool convertToGameChart)
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

    public int NoteScreenTime => (int)((-540f * r_notespeed + 6500) * SpeedCompensationSum);
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