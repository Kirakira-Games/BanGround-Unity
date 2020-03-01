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
    //public static int bufferSize = -1;

    public static bool syncLineEnabled = true;
    public static bool laneEffectEnabled = true;
    public static bool grayNoteEnabled = true;
    public static bool mirrowEnabled = false;
    public static bool bangPerspective = true;
    public static bool autoPlayEnabled = false;

    public static float bgBrightness = .7f;
    public static float laneBrightness = 0.84f;
    public static float longBrightness = .8f;

    public static bool enableAudioTrack = false;

    public const string assetDirectory = "V2Assets";
    public const string IconPath = "UI/ClearMark/";

    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public static Language language = Language.SimplifiedChinese;

    public static int currentChart = 0; // Chart set index
    public static int currentDifficulty = (int)Difficulty.Easy;
    public static int actualDifficulty = currentDifficulty; // These may differ if a chart set does not have currentDifficulty

    public static cHeader CurrentHeader => DataLoader.chartList[currentChart];

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
                cachedScreenTime = (int)(-500 * trueNoteSpeed + 6500);
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
    public int bufferSize = -1;

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

    public int currentChart = 0;
    public int currentDifficulty = 0;

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
        //bufferSize = LiveSetting.bufferSize;
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

        currentChart = LiveSetting.currentChart;
        currentDifficulty = LiveSetting.currentDifficulty;
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
        //LiveSetting.bufferSize = bufferSize;
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

        LiveSetting.currentChart = currentChart;
        LiveSetting.currentDifficulty = currentDifficulty;
    }
}