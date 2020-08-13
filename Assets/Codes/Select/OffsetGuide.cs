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
    private ILiveSetting liveSetting;
    [Inject]
    private IChartListManager chartListManager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        liveSetting.offsetAdjustMode = true;
        chartListManager.ForceChart(liveSetting.offsetAdjustChart, liveSetting.offsetAdjustDiff);

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
