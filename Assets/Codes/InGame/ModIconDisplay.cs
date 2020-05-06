using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class ModIconDisplay : MonoBehaviour
{
    [SerializeField] private Image[] icons;

    static KVarRef mod_autoplay = new KVarRef("mod_autoplay");
    private void Start()
    {
        //icons = GetComponentsInChildren<Image>(true);

        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is DoubleMod) icons[0].gameObject.SetActive(true);
            else if (mod is HalfMod) icons[1].gameObject.SetActive(true);
            else if (mod is SuddenDeathMod) icons[3].gameObject.SetActive(true);
            else if (mod is PerfectMod) icons[4].gameObject.SetActive(true);
            else if (mod is NightCoreMod) icons[5].gameObject.SetActive(true);
            else if (mod is DayCoreMod) icons[6].gameObject.SetActive(true);
        }
        if (mod_autoplay) icons[2].gameObject.SetActive(true);
    }
}
