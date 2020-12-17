﻿using BanGround.Game.Mods;
using BanGround.Scene.Params;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OffsetGuide : MonoBehaviour
{
    public const int OFFSET_GUIDE_SID = 99901;
    public const Difficulty OFFSET_GUIDE_DIFF = Difficulty.Easy;

    [Inject]
    private IKVSystem kvSystem;
    [Inject]
    private IChartLoader chartLoader;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("InGame", () => chartLoader.LoadChart(OFFSET_GUIDE_SID, OFFSET_GUIDE_DIFF, true),
            true, parameters: new InGameParams
            {
                sid = OFFSET_GUIDE_SID,
                difficulty = OFFSET_GUIDE_DIFF,
                mods = ModFlag.None,
                isOffsetGuide = true,
                saveRecord = false,
                saveReplay = false,
            });
    }
}
