using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffsetGuide : MonoBehaviour
{
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
        KVSystem.Instance.SaveConfig();
        SceneLoader.LoadScene("Select", "InGame", () => LiveSetting.LoadChart(true));
    }
}
