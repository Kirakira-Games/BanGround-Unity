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

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        liveSetting.offsetAdjustMode = true;

        //if (!await liveSetting.LoadChart(true))
        //{
        //    liveSetting.offsetAdjustMode = false;

        //    return;
        //}

        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("Select", "InGame", () => liveSetting.LoadChart(true));
    }
}
