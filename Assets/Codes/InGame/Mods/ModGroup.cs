using UnityEngine;
using System.Collections;
using System;
using AudioProvider;

namespace BanGround.Game.Mods
{
    public abstract class ModBase
    {
        public virtual float ScoreMultiplier => 1.0f;
        public virtual Type[] IncompatibleMods => Array.Empty<Type>();
    }

    public abstract class AudioMod : ModBase
    {
        public virtual float SpeedCompensation => 1.0f;
        public virtual void ApplyMod(ISoundTrack soundTrack) { }
    }

    public abstract class PlayMod : ModBase { }
    public class AutoPlayMod : PlayMod
    {
        public static AutoPlayMod Instance = new AutoPlayMod();
    }
    public class SuddenDeathMod : PlayMod
    {
        public static SuddenDeathMod Instance = new SuddenDeathMod();
    }
    public class PerfectMod : PlayMod
    {
        public static PerfectMod Instance = new PerfectMod();
    }

    public class DoubleMod : AudioMod
    {
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
        public static HalfMod Instanse = new HalfMod();

        public override Type[] IncompatibleMods => new Type[] { typeof(DoubleMod) };

        public override float SpeedCompensation => 0.75f;
        public override float ScoreMultiplier => 0.75f;

        public override void ApplyMod(ISoundTrack soundTrack)
        {
            soundTrack.SetTimeScale(0.75f, true);
        }
    }

    public class DayCoreMod : AudioMod
    {
        public static DayCoreMod Instanse = new DayCoreMod();
        public override Type[] IncompatibleMods => new Type[] { typeof(DoubleMod), typeof(HalfMod), typeof(NightCoreMod) };

        public override float SpeedCompensation => 0.75f;
        public override float ScoreMultiplier => 0.75f;

        public override void ApplyMod(ISoundTrack soundTrack)
        {
            soundTrack.SetTimeScale(0.75f, false);
        }
    }
}