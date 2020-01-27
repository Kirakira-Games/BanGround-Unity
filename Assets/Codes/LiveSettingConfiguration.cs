﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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