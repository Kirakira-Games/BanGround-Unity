using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class LiveSetting
{
    public static int judgeOffset = 0;
    public static int audioOffset = 0;

    public static float noteSpeed = 10.8f;
    public static float noteSize = 1f;
    public static float meshSize = .75f;
    public static float meshOpacity = .6f;

    public static float bgmVolume = .7f;
    public static float seVolume = .7f;

    public static bool syncLineEnabled = true;
    public static bool laneEffectEnabled = true;
    public static bool grayNoteEnabled = true;
    public static bool bangPerspective = true;
    public static bool autoPlayEnabled = false;

    public static float bgBrightness = .7f;
    public static float laneBrightness = 0.84f;

    public static string assetDirectory = "V2Assets";

    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;

    public const string testChart = "TestCharts/{0}/0";
    public const string testHeader = "TestCharts/{0}/header";
    public const string testMusic = "TestCharts/{0}/bgm";
    public static string selected = "";
    public static int selectedIndex = 0;

    public static string settingsPath = Application.persistentDataPath + "/LiveSettings.json";

    public static int NoteScreenTime
    {
        get
        {
            if(cachedSpeed != noteSpeed)
            {
                cachedSpeed = noteSpeed;
                cachedScreenTime = (int)(-500 * noteSpeed + 6000);
            }

            return cachedScreenTime;
        }
    }
}

public class LiveSettingTemplate
{
    public int judgeOffset = 0;
    public int audioOffset = 0;

    public  float noteSpeed = 10.8f;
    public  float noteSize = 1f;
    public  float meshSize = .75f;
    public  float meshOpacity = .6f;

    public  float bgmVolume = .7f;
    public  float seVolume = .7f;

    public  bool syncLineEnabled = true;
    public  bool laneEffectEnabled = true;
    public  bool grayNoteEnabled = true;
    public  bool bangPerspective = true;
    public  bool autoPlayEnabled = false;

    public  float bgBrightness = .7f;
    public  float laneBrightness = 0.84f;

    public  int selectedIndex = 0;

    public LiveSettingTemplate()
    {
        judgeOffset = LiveSetting.judgeOffset;
        audioOffset = LiveSetting.audioOffset;
        noteSpeed = LiveSetting.noteSpeed;
        meshSize = LiveSetting.meshSize;
        meshOpacity = LiveSetting.meshOpacity;
        bgmVolume = LiveSetting.bgmVolume;
        seVolume = LiveSetting.seVolume;
        syncLineEnabled = LiveSetting.syncLineEnabled;
        laneEffectEnabled = LiveSetting.laneEffectEnabled;
        grayNoteEnabled = LiveSetting.grayNoteEnabled;
        bangPerspective = LiveSetting.bangPerspective;
        autoPlayEnabled = LiveSetting.autoPlayEnabled;

        bgBrightness = LiveSetting.bgBrightness;
        laneBrightness = LiveSetting.laneBrightness;

        selectedIndex = LiveSetting.selectedIndex;
    }
    public static void ApplyToLiveSetting(LiveSettingTemplate st)
    {

        LiveSetting.judgeOffset = st.judgeOffset;
        LiveSetting.audioOffset = st.audioOffset;
        LiveSetting.noteSpeed = st.noteSpeed;
        LiveSetting.meshSize = st.meshSize;
        LiveSetting.meshOpacity = st.meshOpacity;
        LiveSetting.bgmVolume = st.bgmVolume;
        LiveSetting.seVolume = st.seVolume;
        LiveSetting.syncLineEnabled = st.syncLineEnabled;
        LiveSetting.laneEffectEnabled = st.laneEffectEnabled;
        LiveSetting.grayNoteEnabled = st.grayNoteEnabled;
        LiveSetting.bangPerspective = st.bangPerspective;
        LiveSetting.autoPlayEnabled = st.autoPlayEnabled;

        LiveSetting.bgBrightness = st.bgBrightness;
        LiveSetting.laneBrightness = st.laneBrightness;

        LiveSetting.selectedIndex = st.selectedIndex;
    }
}