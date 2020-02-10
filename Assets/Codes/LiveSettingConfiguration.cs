using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class LiveSetting
{
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

    public const string assetDirectory = "V2Assets";
    public const string IconPath = "UI/v3/";

    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

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

    public static string GetChartPath => ChartDir + selectedFolder + "/" + selectedChart + ".json";
    public static string GetBGMPath => ChartDir + selectedFolder + "/" + "bgm.mp3";
    public static string GetHeaderPath => ChartDir + selectedFolder + "/" + "header.json";
    public static string GetPreviewMusicPath => ChartDir + selectedFolder + "/" + "preview.wav";
    public static string GetBackgroundPath => ChartDir + selectedFolder + "/" + selectedChart + ".jpg";
    public static Header CurrentHeader;

    public static readonly string settingsPath = Application.persistentDataPath + "/LiveSettings.json";
    public static readonly string scoresPath = Application.persistentDataPath + "/Scores.json";

    public static int NoteScreenTime
    {
        get
        {
            if(cachedSpeed != trueNoteSpeed)
            {
                cachedSpeed = trueNoteSpeed;
                cachedScreenTime = (int)(-490 * trueNoteSpeed + 5990);
            }

            return cachedScreenTime;
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
    }
    public static void ApplyToLiveSetting(LiveSettingTemplate st)
    {

        LiveSetting.judgeOffset = st.judgeOffset;
        LiveSetting.audioOffset = st.audioOffset;
        LiveSetting.noteSpeed = st.noteSpeed;
        LiveSetting.noteSize = st.noteSize;
        LiveSetting.meshSize = st.meshSize;
        LiveSetting.meshOpacity = st.meshOpacity;
        LiveSetting.bgmVolume = st.bgmVolume;
        LiveSetting.seVolume = st.seVolume;
        LiveSetting.syncLineEnabled = st.syncLineEnabled;
        LiveSetting.laneEffectEnabled = st.laneEffectEnabled;
        LiveSetting.grayNoteEnabled = st.grayNoteEnabled;
        LiveSetting.mirrowEnabled = st.mirrowEnabled;
        LiveSetting.bangPerspective = st.bangPerspective;
        LiveSetting.autoPlayEnabled = st.autoPlayEnabled;

        LiveSetting.bgBrightness = st.bgBrightness;
        LiveSetting.laneBrightness = st.laneBrightness;
        LiveSetting.longBrightness = st.longBrightness;
    }
}