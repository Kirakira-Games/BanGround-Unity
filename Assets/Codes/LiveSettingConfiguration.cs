using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public static class LiveSetting
{
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
            loaded.ApplyToLiveSetting();
        }
        else
        {
            Debug.LogWarning("Live setting file not found");
        }
    }

    public static int judgeOffset = 0;
    public static int audioOffset = 0;

    public static float noteSpeed = 10.8f;
    private static float trueNoteSpeed => noteSpeed;
    public static float noteSize = 1f;
    public static float meshSize = .75f;
    public static float meshOpacity = .6f;

    public static float bgmVolume = .7f;
    public static float seVolume = .7f;

    public static bool syncLineEnabled = true;
    public static bool laneEffectEnabled = true;
    public static bool grayNoteEnabled = true;
    public static bool mirrowEnabled = false;
    public static bool bangPerspective = true;
    public static bool autoPlayEnabled = false;

    public static float bgBrightness = .7f;
    public static float laneBrightness = 0.84f;
    public static float longBrightness = .8f;

    public static bool enableAudioTrack = true;

    public const string assetDirectory = "V2Assets";
    public const string IconPath = "UI/v3/";

    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public static Language language = Language.SimplifiedChinese;

#if UNITY_ANDROID && !UNITY_EDITOR
    public static readonly string ChartDir = Application.persistentDataPath + "/TestCharts/";
#else 
    public static readonly string ChartDir = Application.streamingAssetsPath + "/TestCharts/";
#endif

    public static string selectedChart = "0";//file name
    public static string selectedFolder = "";
    public static int selectedIndex = 0;
    public static int selectedDifficulty = (int)Difficulty.Easy;

    public static SongList songList;

    public static string GetChartPath => ChartDir + selectedFolder + "/" + selectedChart + ".bin";
    public static string GetBGMPath => ChartDir + selectedFolder + "/" + "bgm.ogg";
    public static string GetHeaderPath => ChartDir + selectedFolder + "/" + "header.bin";
    public static string GetBackgroundPath => ChartDir + selectedFolder + "/" + selectedChart + ".jpg";
    public static Header CurrentHeader;

    public static readonly string settingsPath = Application.persistentDataPath + "/LiveSettings.json";
    public static readonly string scoresPath = Application.persistentDataPath + "/Scores.bin";

    public static List<ModBase> attachedMods = new List<ModBase>();
    public static float SpeedCompensationSum = 1.0f;

    public static bool AddMod(ModBase mod)
    {
        if (!attachedMods.Contains(mod))
        {
            if (attachedMods.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(mod))))
                return false;

            attachedMods.Add(mod);
            if(mod is AudioMod)
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

    public static int NoteScreenTime
    {
        get
        {
            if(cachedSpeed != trueNoteSpeed)
            {
                cachedSpeed = trueNoteSpeed;
                cachedScreenTime = (int)(-490 * trueNoteSpeed + 5990);
            }

            return (int)(cachedScreenTime * SpeedCompensationSum);
        }
    }
}

public class LiveSettingTemplate
{
    public int judgeOffset = 0;
    public int audioOffset = 0;

    public float noteSpeed = 10.8f;
    public float noteSize = 1f;
    public float meshSize = .75f;
    public float meshOpacity = .6f;

    public float bgmVolume = .7f;
    public float seVolume = .7f;

    public bool syncLineEnabled = true;
    public bool laneEffectEnabled = true;
    public bool grayNoteEnabled = true;
    public bool mirrowEnabled = false;
    public bool bangPerspective = true;
    public bool autoPlayEnabled = false;

    public float bgBrightness = .7f;
    public float laneBrightness = 0.84f;
    public float longBrightness = .8f;

    public bool enableAudioTrack = true;

    public int selectedIndex = 0;

    public LiveSettingTemplate()
    {
        judgeOffset = LiveSetting.judgeOffset;
        audioOffset = LiveSetting.audioOffset;
        noteSpeed = LiveSetting.noteSpeed;
        noteSize = LiveSetting.noteSize;
        meshSize = LiveSetting.meshSize;
        meshOpacity = LiveSetting.meshOpacity;
        bgmVolume = LiveSetting.bgmVolume;
        seVolume = LiveSetting.seVolume;
        syncLineEnabled = LiveSetting.syncLineEnabled;
        laneEffectEnabled = LiveSetting.laneEffectEnabled;
        grayNoteEnabled = LiveSetting.grayNoteEnabled;
        mirrowEnabled = LiveSetting.mirrowEnabled;
        bangPerspective = LiveSetting.bangPerspective;
        autoPlayEnabled = LiveSetting.autoPlayEnabled;

        bgBrightness = LiveSetting.bgBrightness;
        laneBrightness = LiveSetting.laneBrightness;
        longBrightness = LiveSetting.longBrightness;

        enableAudioTrack = LiveSetting.enableAudioTrack;

        selectedIndex = LiveSetting.selectedIndex;
    }
    public void ApplyToLiveSetting()
    {

        LiveSetting.judgeOffset = judgeOffset;
        LiveSetting.audioOffset = audioOffset;
        LiveSetting.noteSpeed = noteSpeed;
        LiveSetting.noteSize = noteSize;
        LiveSetting.meshSize = meshSize;
        LiveSetting.meshOpacity = meshOpacity;
        LiveSetting.bgmVolume = bgmVolume;
        LiveSetting.seVolume = seVolume;
        LiveSetting.syncLineEnabled = syncLineEnabled;
        LiveSetting.laneEffectEnabled = laneEffectEnabled;
        LiveSetting.grayNoteEnabled = grayNoteEnabled;
        LiveSetting.mirrowEnabled = mirrowEnabled;
        LiveSetting.bangPerspective = bangPerspective;
        LiveSetting.autoPlayEnabled = autoPlayEnabled;

        LiveSetting.bgBrightness = bgBrightness;
        LiveSetting.laneBrightness = laneBrightness;
        LiveSetting.longBrightness = longBrightness;

        LiveSetting.enableAudioTrack = enableAudioTrack;

        LiveSetting.selectedIndex = selectedIndex;
    }
}