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

        //if (!await liveSetting.LoadChart(true))
        //{
        //    liveSetting.offsetAdjustMode = false;

        //    return;
        //}

        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("Select", "InGame", async () =>
        {
            if (!await chartListManager.LoadChart(true))
            {
                return false;
            }
            return true;
        });
    }
}
