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

    public string assetDirectory => fs_assetpath;
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