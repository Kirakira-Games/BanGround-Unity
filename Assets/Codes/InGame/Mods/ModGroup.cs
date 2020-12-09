using UnityEngine;
using System.Collections;
using System;
using AudioProvider;

public abstract class ModBase
{
    public abstract ulong Flag { get; }
    public virtual float ScoreMultiplier => 1.0f;
    public virtual Type[] IncompatibleMods => Array.Empty<Type>();
}

public abstract class AudioMod : ModBase
{
    public virtual float SpeedCompensation => 1.0f;
    public virtual void ApplyMod(ISoundTrack soundTrack){}
}

public abstract class PlayMod : ModBase { }
public class AutoPlayMod : PlayMod
{
    public override ulong Flag => 1ul << 63;
    public static AutoPlayMod Instance = new AutoPlayMod();
}
public class SuddenDeathMod : PlayMod
{
    public override ulong Flag => 1ul << 0;
    public static SuddenDeathMod Instance = new SuddenDeathMod();
}
public class PerfectMod : PlayMod
{
    public override ulong Flag => 1ul << 1;
    public static PerfectMod Instance = new PerfectMod();
}

public class DoubleMod : AudioMod
{
    public override ulong Flag => 1ul << 2;
    public static DoubleMod Instanse = new DoubleMod();

    public override Type[] IncompatibleMods => new Type[] { typeof(HalfMod) };

    public override float SpeedCompensation => 1.5f;
    public override float ScoreMultiplier => 1.1f;

    public override void ApplyMod(ISoundTrack soundTrack)
    {
        soundTrack.SetTimeScale(1.5f, true);
    }
}

public class NightCoreMod : AudioMod
{
    public override ulong Flag => 1ul << 3;
    public static NightCoreMod Instanse = new NightCoreMod();

    public override Type[] IncompatibleMods => new Type[] { typeof(HalfMod), typeof(DoubleMod), typeof(DayCoreMod) };
    public override float SpeedCompensation => 1.5f;
    public override float ScoreMultiplier => 1.1f;

    public override void ApplyMod(ISoundTrack soundTrack)
    {
        soundTrack.SetTimeScale(1.5f, false);
    }
}

public class HalfMod : AudioMod
{
    public override ulong Flag => 1ul << 4;
    public static HalfMod Instanse = new HalfMod();

    public override Type[] IncompatibleMods => new Type[] { typeof(DoubleMod) };

    public override float SpeedCompensation => 0.75f;
    public override float ScoreMultiplier => 0.75f;

    public override void ApplyMod(ISoundTrack soundTrack)
    {
        soundTrack.SetTimeScale(0.75f, true);
    }
}

public class DayCoreMod: AudioMod
{
    public override ulong Flag => 1ul << 5;
    public static DayCoreMod Instanse = new DayCoreMod();
    public override Type[] IncompatibleMods => new Type[] { typeof(DoubleMod), typeof(HalfMod), typeof(NightCoreMod) };

    public override float SpeedCompensation => 0.75f;
    public override float ScoreMultiplier => 0.75f;

    public override void ApplyMod(ISoundTrack soundTrack)
    {
        soundTrack.SetTimeScale(0.75f, false);
    }
}