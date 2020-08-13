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

    //public static bool fullScreen;
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