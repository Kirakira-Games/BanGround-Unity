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

    async void StartOffsetGuide()
    {
        if (!await LiveSetting.LoadChart())
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, "This chart is outdated and unsupported.");
            return;
        }

        LiveSetting.offsetAdjustMode = true;
        SettingAndMod.instance.SetLiveSetting();
        KVSystem.Instance.SaveConfig();
        SceneLoader.LoadScene("NewSelect", "InGame", true);
    }
}
