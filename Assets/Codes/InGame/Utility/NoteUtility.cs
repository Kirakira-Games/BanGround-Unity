﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameNoteType
{
    None = -1,
    Normal = 0,
    Flick = 1,
    SlideStart = 2,
    SlideTick = 3,
    SlideEnd = 4,
    SlideEndFlick = 5
}

public class GameNoteData
{
    public int time;
    public int lane;
    public GameNoteType type;
    public bool isGray;
    public List<GameNoteData> seg;
}

public enum JudgeResult
{
    None = -1,
    Perfect,
    Great,
    Good,
    Bad,
    Miss
}

public static class NoteUtility
{
    public const int LANE_COUNT = 7;
    public const float NOTE_START_POS = 365;
    public const float NOTE_JUDGE_POS = 20;
    public const float NOTE_Y_POS = -105.5f;
    public const float LANE_WIDTH = 1.366f;
    public const float NOTE_SCALE = 0.8f;

    private static readonly float BANG_PERSPECTIVE_START = YTo3DXHelper(0);
    private static readonly float BANG_PERSPECTIVE_END = YTo3DXHelper(1);
    private static readonly float BANG_EXP_START = XToBanGYHelper(0);

    public static readonly int[] TAP_JUDGE_RANGE = { 50, 100, 117, 133 };
    public static readonly int[] SLIDE_END_JUDGE_RANGE = { 67, 117, 133, 150 };
    public const int SLIDE_END_FLICK_JUDGE_RANGE = 100;
    public const int SLIDE_TICK_JUDGE_RANGE = 200;
    public const int AUTO_JUDGE_RANGE = 10;
    public const float FLICK_JUDGE_DIST = 0.5f;

    public const int MOUSE_TOUCH_ID = -16;

    public const float EPS = 1e-4f;

    public static Vector3 GetInitPos(int lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_START_POS);
    }

    public static Vector3 GetJudgePos(int lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_JUDGE_POS);
    }

    public static bool IsFlick(GameNoteType type)
    {
        return type == GameNoteType.Flick ||
            type == GameNoteType.SlideEndFlick;
    }

    public static bool IsSlide(GameNoteType type)
    {
        return type == GameNoteType.SlideStart ||
            type == GameNoteType.SlideTick ||
            IsSlideEnd(type);
    }

    public static bool IsSlideEnd(GameNoteType type)
    {
        return type == GameNoteType.SlideEnd ||
            type == GameNoteType.SlideEndFlick;
    }

    public static void Shuffle<T>(List<T> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            int index = Random.Range(0, i + 1);
            T temp = input[index];
            input[index] = input[i];
            input[i] = temp;
        }
    }

    public static T LoadResource<T>(string name) where T: Object
    {
        return Resources.Load<T>(LiveSetting.assetDirectory + "/" + name);
    }

    public static bool IsTouchContinuing(Touch touch)
    {
        return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
    }

    public static bool IsTouchEnd(Touch touch)
    {
        return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
    }

    private static float XToBanGYHelper(float x)
    {
        return Mathf.Pow(1.1f, -50 * (1-x));
    }

    private static float XToBanGY(float x)
    {
        return (XToBanGYHelper(x) - BANG_EXP_START) / (1 - BANG_EXP_START);
    }

    private static float YTo3DXHelper(float x)
    {
        return -1.02458329f / Mathf.Pow(3.90425367f * Mathf.Pow(x, 0.9f) + 0.99125229f, 2f) + 1.04274808f;
    }

    private static float YTo3DX(float x)
    {
        return (YTo3DXHelper(x) - BANG_PERSPECTIVE_START) / (BANG_PERSPECTIVE_END - BANG_PERSPECTIVE_START);
    }

    public static float GetBangPerspective(float x)
    {
        if (x <= 0) return 0;
        return YTo3DX(XToBanGY(x));
    }
}
