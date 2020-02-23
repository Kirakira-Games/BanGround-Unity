using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModIconDisplay : MonoBehaviour
{
    private Image[] icons;

    private void Start()
    {
        icons = GetComponentsInChildren<Image>(true);

        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is DoubleMod) icons[0].gameObject.SetActive(true);
            else if (mod is HalfMod) icons[1].gameObject.SetActive(true);
            else if (mod is SuddenDeathMod) icons[3].gameObject.SetActive(true);
            else if (mod is PerfectMod) icons[4].gameObject.SetActive(true);
        }
        if (LiveSetting.autoPlayEnabled) icons[2].gameObject.SetActive(true);
    }
}
