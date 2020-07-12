using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UniRx.Async;

public static class LiveSetting
{
    /*
    public static bool Loaded { get; private set; } = false;
    public static void Load()
    {
        if (Loaded)
            return;

        Loaded = true;

        if (File.Exists(settingsPath))
        {
            string sets = File.ReadAllText(settingsPath);
            LiveSettingTemplate loaded = JsonConvert.DeserializeObject<LiveSettingTemplate>(sets);
        }
        else
        {
            Debug.LogWarning("Live setting file not found");
        }

        if(language == Language.AutoDetect)
        {
            switch(Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    language = Language.SimplifiedChinese;
                    break;
                case SystemLanguage.ChineseTraditional:
                    language = Language.TraditionalChinese;
                    break;
                case SystemLanguage.Japanese:
                    language = Language.Japanese;
                    break;
                case SystemLanguage.Korean:
                    language = Language.Korean;
                    break;
                default:
                    language = Language.English;
                    break;
            }
        }

        AudioManager.Provider.SetSoundEffectVolume(seVolume, AudioProvider.SEType.Common);
        AudioManager.Provider.SetSoundEffectVolume(igseVolume, AudioProvider.SEType.InGame);
        AudioManager.Provider.SetSoundTrackVolume(bgmVolume);
    }
    */
    /*
    public static int judgeOffset = 0;
    public static int audioOffset = 0;

    public static float noteSpeed = 10f;
    private static float trueNoteSpeed => noteSpeed;
    public static float noteSize = 1f;
    public static float meshSize = .75f;
    public static float meshOpacity = .6f;

    public static float bgmVolume = .7f;
    public static float seVolume = .7f;
    public static float igseVolume = .7f;
    //public static int bufferSize = -1;

    public static bool syncLineEnabled = true;
    public static bool laneEffectEnabled = true;
    public static bool grayNoteEnabled = true;
    public static bool mirrowEnabled = false;
    public static bool bangPerspective = true;
    public static bool autoPlayEnabled = false;
    public static bool laneLight = true;
    public static bool shakeFlick = true;
    public static bool dispMilisec = false;

    public static float ELPValue = 0;
    public static float offsetTransform = 1f;
    public static float farClip = 169f;
    public static float bgBrightness = .7f;
    public static float laneBrightness = 0.84f;
    public static float longBrightness = .8f;
    */

    /*
   public static NoteStyle noteStyle = NoteStyle.Circle;
   public static SEStyle seStyle = SEStyle.Drum;


   public static Language language = Language.AutoDetect;
   public static Sorter sort = Sorter.SongName;
   */

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

    public static string assetDirectory => fs_assetpath;
    public static string IconPath => fs_iconpath;
    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public static int currentChart = 0; // Chart set index
    public static int currentDifficulty = (int)Difficulty.Easy;
    private static int cachedActualDifficulty = currentDifficulty;
    public static int actualDifficulty
    {
        get
        {
            return offsetAdjustMode ? (int)Difficulty.Normal : cachedActualDifficulty;
        }
        set
        {
            cachedActualDifficulty = value;
        }
    } // actualDifficulty may differ if a chart set does not have currentDifficulty

    public const int offsetAdjustChart = 9746;
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
    public static Chart chart;

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

    public static async UniTask<bool> LoadChart()
    {
        chart = DataLoader.LoadChart(CurrentHeader.sid, (Difficulty)actualDifficulty);
        if (!await ChartVersion.Process(CurrentHeader, chart))
        {
            chart = null;
            return false;
        }
        return true;
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