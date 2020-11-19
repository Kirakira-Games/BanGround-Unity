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
    [Inject]
    private IModManager modManager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        chartListManager.ForceOffsetChart();
        modManager.SuppressAllMods(true);

        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("Select", "InGame", async () =>
        {
            if (!await chartListManager.LoadChart(true))
            {
                modManager.SuppressAllMods(false);
                chartListManager.ClearForcedChart();
                return false;
            }
            return true;
        });
    }
}
