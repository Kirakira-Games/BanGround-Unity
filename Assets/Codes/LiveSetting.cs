using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UniRx.Async;
using System;
using Zenject;

public class LiveSetting : ILiveSetting
{
    public static ILiveSetting Instance;

    [Inject(Id = "fs_assetpath")]
    private KVar fs_assetpath;
    [Inject(Id = "fs_iconpath")]
    private KVar fs_iconpath;
    [Inject(Id = "r_notespeed")]
    private KVar r_notespeed;

    private DemoFile _demoFile = null;

    public DemoFile DemoFile
    {
        get
        {
            var demo = _demoFile;
            _demoFile = null;
            return demo;
        }
        set
        {
            _demoFile = value;
        }
    }

    public string assetDirectory => fs_assetpath;
    public string IconPath => fs_iconpath;

    public static readonly string settingsPath = Application.persistentDataPath + "/LiveSettings.json";
    public static readonly string scoresPath = Application.persistentDataPath + "/Scores.bin";

    public List<ModBase> attachedMods { get; set; } = new List<ModBase>();
    public float SpeedCompensationSum { get; set; } = 1.0f;

    //public static bool fullScreen;

    public bool AddMod(ModBase mod)
    {
        if (mod == null) return false;
        if (!attachedMods.Contains(mod))
        {
            if (attachedMods.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(mod))))
                return false;

            attachedMods.Add(mod);
            if (mod is AudioMod)
            {
                SpeedCompensationSum *= (mod as AudioMod).SpeedCompensation;
            }
        }

        return true;
    }

    public void RemoveMod(ModBase mod)
    {
        if (attachedMods.Contains(mod))
        {
            attachedMods.Remove(mod);

            if (mod is AudioMod)
            {
                SpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public void RemoveAllMods()
    {
        foreach (var mod in attachedMods)
        {
            if (mod is AudioMod)
            {
                SpeedCompensationSum /= (mod as AudioMod).SpeedCompensation;
            }
        }
    }

    public int NoteScreenTime => (int)((-540f * r_notespeed + 6500) * SpeedCompensationSum);
}

public enum NoteStyle
{
    Circle,
    Cube,
    Dark,
    Custom
}

public enum SEStyle
{
    None,
    Drum,
    Bbben,
    AECBanana,
    Custom
}