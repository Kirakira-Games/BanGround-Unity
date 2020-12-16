using BanGround.Game.Mods;
using BanGround.Scene.Params;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OffsetGuide : MonoBehaviour
{
    [Inject]
    private IKVSystem kvSystem;
    [Inject]
    private IChartListManager chartListManager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        chartListManager.ForceOffsetChart();

        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("InGame", async () =>
        {
            if (!await chartListManager.LoadChart(true, ModFlag.None))
            {
                chartListManager.ClearForcedChart();
                return false;
            }
            return true;
        }, true, parameters: new InGameParams
        {
            mods = ModFlag.None,
            saveRecord = false,
            saveReplay = false,
        });
    }
}
