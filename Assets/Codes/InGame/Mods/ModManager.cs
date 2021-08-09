using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using System;

namespace BanGround.Game.Mods
{
    [Flags]
    public enum ModFlag : ulong
    {
        None = 0,
        AutoPlay = 1ul << 63,
        SuddenDeath = 1ul << 0,
        Perfect = 1ul << 1,
        Double = 1ul << 2,
        NightCore = 1ul << 3,
        Half = 1ul << 4,
        DayCore = 1ul << 5,
        Mirror = 1ul << 6
    }

    public static class ModFlagUtil
    {
        private const int BASE = 16;

        public static ModFlag From(KVar kvar)
        {
            return (ModFlag)Convert.ToUInt64(kvar, BASE);
        }

        public static void SetMod(this KVar kvar, ModFlag flag)
        {
            // No override for ulong, but the hex pattern should be the same
            kvar.Set(Convert.ToString((long)flag, BASE));
        }

        public static List<ModBase> Create(this ModFlag flag)
        {
            var ret = new List<ModBase>();
            if (flag.HasFlag(ModFlag.AutoPlay))
                ret.Add(new AutoPlayMod());
            if (flag.HasFlag(ModFlag.SuddenDeath))
                ret.Add(new SuddenDeathMod());
            if (flag.HasFlag(ModFlag.Perfect))
                ret.Add(new PerfectMod());
            if (flag.HasFlag(ModFlag.Double))
                ret.Add(new DoubleMod());
            if (flag.HasFlag(ModFlag.NightCore))
                ret.Add(new NightCoreMod());
            if (flag.HasFlag(ModFlag.Half))
                ret.Add(new HalfMod());
            if (flag.HasFlag(ModFlag.DayCore))
                ret.Add(new DayCoreMod());
            if (flag.HasFlag(ModFlag.Mirror))
                ret.Add(new MirrorMod());
            return ret;
        }
    }

    public class ModManager : IModManager
    {
        private KVar r_notespeed;

        public List<ModBase> AttachedMods { get; private set; } = new List<ModBase>();
        public float SpeedCompensationSum { get; private set; }
        public int NoteScreenTime => (int)((-540f * r_notespeed + 6500) * SpeedCompensationSum);

        public float ScoreMultiplier
        {
            get => AttachedMods.Aggregate(1f, (acc, x) => acc * x.ScoreMultiplier);
        }

        public ModFlag Flag { get; private set; } = 0;

        public ModManager(KVar r_notespeed, ModFlag mods)
        {
            AttachedMods.Clear();
            this.r_notespeed = r_notespeed;
            SpeedCompensationSum = 1f;
            Flag = mods;
            
            // Create mod instances
            var modInstances = mods.Create();
            foreach (var mod in modInstances)
            {
                if (!AddMod(mod))
                {
                    Debug.LogError($"[ModManager] {mod.GetType().Name}");
                }
            }
        }

        private bool AddMod(ModBase mod)
        {
            if (mod == null) return false;
            if (!AttachedMods.Contains(mod))
            {
                if (AttachedMods.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(mod))))
                    return false;

                AttachedMods.Add(mod);
                if (mod is AudioMod)
                {
                    SpeedCompensationSum *= (mod as AudioMod).SpeedCompensation;
                }
            }

            return true;
        }
    }
}
