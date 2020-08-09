using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OffsetGuide : MonoBehaviour
{
    [Inject]
    IKVSystem kvSystem;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartOffsetGuide);
    }

    void StartOffsetGuide()
    {
        LiveSetting.offsetAdjustMode = true;

        //if (!await LiveSetting.LoadChart(true))
        //{
        //    LiveSetting.offsetAdjustMode = false;

        //    return;
        //}

        SettingAndMod.instance.SetLiveSetting();
        kvSystem.SaveConfig();
        SceneLoader.LoadScene("Select", "InGame", () => LiveSetting.LoadChart(true));
    }
}
