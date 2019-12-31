using UnityEngine;
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
    public const float NOTE_SCALE = 0.5f;

    public static readonly int[] TAP_JUDGE_RANGE = { 50, 100, 117, 133 };
    public static readonly int[] SLIDE_END_JUDGE_RANGE = { 67, 117, 133, 150 };
    public const int SLIDE_END_FLICK_JUDGE_RANGE = 100;
    public const int SLIDE_TICK_JUDGE_RANGE = 200;
    public const int AUTO_JUDGE_RANGE = 10;
    public const float FLICK_JUDGE_DIST = 0.5f;

    public const int MOUSE_TOUCH_ID = 1919810;

    public static Vector3 GetInitPos(int lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_START_POS);
    }

    public static Vector3 GetJudgePos(int lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_JUDGE_POS);
    }

    public static bool IsSlide(GameNoteType type)
    {
        return type == GameNoteType.SlideStart ||
            type == GameNoteType.SlideTick ||
            type == GameNoteType.SlideEnd ||
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
}
