using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameNoteType
{
    None = -1,
    Single = 0,
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
    public Vector3 pos;
    public GameNoteType type;
    public bool isGray;
    public bool isFuwafuwa;
    public List<GameNoteData> seg;
    public List<V2.NoteAnim> anims;
    public int appearTime;
    public int timingGroup;

    public void ComputeTime()
    {
        appearTime = seg[0].appearTime;
        foreach (var i in seg)
        {
            appearTime = Mathf.Min(appearTime, i.appearTime);
            time = i.time;
        }
    }
}

public class GameTimingGroup
{
    public List<V2.TimingPoint> points = new List<V2.TimingPoint>();
}

public class GameChartData
{
    public bool isFuwafuwa;
    public int numNotes;
    public GameNoteData[] notes;
    public GameTimingGroup[] groups;
    public V2.ValuePoint[] bpm;
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

public enum TapEffectType
{
    Perfect,
    Great,
    Good,
    Click,
    Flick,
    None
}

public static class NoteUtility
{
    public const int LANE_COUNT = 7;
    public const float NOTE_START_Z_POS = 200;
    public const float NOTE_JUDGE_Z_POS = 8;
    public const float NOTE_Y_POS = 0f;
    public const float NOTE_Y_MAX = 3.5f;
    public const float LANE_WIDTH = 2f;
    public const float LANE_JUDGE_WIDTH = 2.8f;
    public const float LANE_JUDGE_HEIGHT = 2.5f;
    public const float NOTE_SCALE = 1f;
    public const float FUWAFUWA_RADIUS = 2.8f;
    public static Plane JudgePlane;

    private static readonly float BANG_PERSPECTIVE_START = YTo3DXHelper(0);
    private static readonly float BANG_PERSPECTIVE_END = YTo3DXHelper(1);
    private static readonly float BANG_EXP_START = XToBanGYHelper(0);

    private static readonly int[] TAP_JUDGE_RANGE_RAW = { 40, 90, 111, 133 };
    private static readonly int[] SLIDE_END_JUDGE_RANGE_RAW = { 57, 107, 128, 150 };
    private static readonly int[] SLIDE_END_TILT_JUDGE_RANGE_RAW = { -34, -84, -100, -117 };
    private const int SLIDE_END_FLICK_JUDGE_RANGE_RAW = 100;
    private const int SLIDE_TICK_JUDGE_RANGE_RAW = 200;

    public static readonly int[] TAP_JUDGE_RANGE = new int[4];
    public static readonly int[] SLIDE_END_JUDGE_RANGE = new int[4];
    public static readonly int[] SLIDE_END_TILT_JUDGE_RANGE = new int[4];
    public static int SLIDE_END_FLICK_JUDGE_RANGE => (int)(SLIDE_END_FLICK_JUDGE_RANGE_RAW * LiveSetting.SpeedCompensationSum);
    public static int SLIDE_TICK_JUDGE_RANGE => (int)(SLIDE_TICK_JUDGE_RANGE_RAW * LiveSetting.SpeedCompensationSum);

    public const float FLICK_JUDGE_DIST = 0.8f;

    public const int MOUSE_TOUCH_ID = -16;

    public const float EPS = 1e-4f;

    private static float planeDeltaZ;
    private static float planeInitZ;
    public static void Init(Vector3 planeNormal)
    {
        JudgePlane = new Plane(planeNormal.normalized, new Vector3(0, 0, NOTE_JUDGE_Z_POS));
        for (int i = 0; i < TAP_JUDGE_RANGE.Length; i++)
        {
            TAP_JUDGE_RANGE[i] = (int)(TAP_JUDGE_RANGE_RAW[i] * LiveSetting.SpeedCompensationSum);
            SLIDE_END_JUDGE_RANGE[i] = (int)(SLIDE_END_JUDGE_RANGE_RAW[i] * LiveSetting.SpeedCompensationSum);
            SLIDE_END_TILT_JUDGE_RANGE[i] = (int)(SLIDE_END_TILT_JUDGE_RANGE_RAW[i] * LiveSetting.SpeedCompensationSum);
        }
        planeInitZ = ProjectVectorToParallelPlane(new Vector3(0, 0)).z;
        planeDeltaZ = ProjectVectorToParallelPlane(new Vector3(0, 1)).z - planeInitZ;
    }

    public static Vector3 GetInitPos(float lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_START_Z_POS);
    }

    public static Vector3 GetJudgePos(float lane)
    {
        return new Vector3((lane - 3) * LANE_WIDTH, NOTE_Y_POS, NOTE_JUDGE_Z_POS);
    }

    public static Vector3 ProjectVectorToParallelPlane(Vector3 position)
    {
        JudgePlane.Raycast(new Ray(
            new Vector3(position.x, position.y, 0),
            new Vector3(0, 0, 1)), out float dist);
        dist -= NOTE_JUDGE_Z_POS;
        position.z += dist;
        return position;
    }

    public static float GetDeltaZFromJudgePlane(float y)
    {
        return planeDeltaZ * y;
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

    static KVarRef cl_notestyle = new KVarRef("cl_notestyle");

    public static T LoadResource<T>(string name) where T: Object
    {
        return Resources.Load<T>(LiveSetting.assetDirectory + "/" + System.Enum.GetName(typeof(NoteStyle), (NoteStyle)cl_notestyle) +"/"+ name);
    }
    public static Object LoadResource(string name) 
    {
        return Resources.Load(LiveSetting.assetDirectory + "/" + System.Enum.GetName(typeof(NoteStyle), (NoteStyle)cl_notestyle) + "/" + name);
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
        return -1.02321179f / (17.47616718f * x + 0.96943588f) + 1.05546995f;
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

    public static float GetXPos(float lane)
    {
        return (lane - 3) * LANE_WIDTH;
    }

    public static float GetYPos(float y)
    {
        return Mathf.LerpUnclamped(NOTE_Y_POS, NOTE_Y_MAX, y);
    }

    public static float Interpolate(float inS, float inT, float inP, float outS, float outT)
    {
        if (inS == inT) return outS;
        float ratio = (inP - inS) / (inT - inS);
        return Mathf.Lerp(outS, outT, ratio);
    }
}
