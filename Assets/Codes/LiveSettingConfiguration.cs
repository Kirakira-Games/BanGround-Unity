using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LiveSetting
{
    public static float offset = .0f;

    public static float noteSpeed = 5.0f;
    public static float noteSize = 1.0f;
    public static float meshSize = .75f;
    public static float meshOpacity = .6f;

    public static float bgmVolume = .7f;
    public static float seVolume = .7f;

    public static bool syncLineEnabled = true;
    public static bool laneEffectEnabled = true;

    private static float cachedSpeed = 0;
    private static int cachedScreenTime = 0;
    
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