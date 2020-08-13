using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class ModManager : IModManager
{
    public static IModManager Instance; // TODO: Remove
    [Inject(Id = "r_notespeed")]
    private KVar r_notespeed;

    public bool isSuppressingMods { get; private set; } = false;

    public bool isAutoplay => attachedMods.Any(mod => mod is AutoPlayMod);

    private List<ModBase> mAttachedMods = new List<ModBase>();
    private List<ModBase> emptyMods = new List<ModBase>();
    public List<ModBase> attachedMods => isSuppressingMods ? emptyMods : mAttachedMods;

    private float mSpeedCompensationSum = 1.0f;
    public float SpeedCompensationSum => isSuppressingMods ? 1.0f : mSpeedCompensationSum;
    public int NoteScreenTime => (int)((-540f * r_notespeed + 6500) * SpeedCompensationSum);

    // TODO: Remove
    public ModManager()
    {
        Instance = this;
    }

    public bool AddMod(ModBase mod)
    {
        if (mod == null) return false;
        if (!mAttachedMods.Contains(mod))
        {
            if (mAttachedMods.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(mod))))
                return false;

            mAttachedMods.Add(mod);
            if (mod is AudioMod)
            {
                mSpeedCompensationSum *= (mod as AudioMod).SpeedCompensation;
            }
        }

        return true;
    }

    public void RemoveMod(ModBase mod)
    {
        if (mAttachedMods.Contains(mod))
        {
            mAttachedMods.Remove(mod);

            if (mod is AudioMod)
            {
                mSpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public void RemoveAllMods()
    {
        foreach (var mod in mAttachedMods)
        {
            if (mod is AudioMod)
            {
                mSpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
        mAttachedMods.Clear();
    }

    public void SuppressAllMods(bool suppress)
    {
        isSuppressingMods = suppress;
    }
}
