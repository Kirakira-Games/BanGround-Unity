using BanGround.Game.Mods;
using BanGround.Scene.Params;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class ModIconDisplay : MonoBehaviour
{
    [SerializeField]
    private Image[] icons;

    private void Start()
    {
        //icons = GetComponentsInChildren<Image>(true);
        var flag = SceneLoader.GetParamsOrDefault<InGameParams>().mods;
        if (flag.HasFlag(ModFlag.Double))
            icons[0].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.Half))
            icons[1].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.AutoPlay))
            icons[2].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.SuddenDeath))
            icons[3].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.Perfect))
            icons[4].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.NightCore))
            icons[5].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.DayCore))
            icons[6].gameObject.SetActive(true);
        if (flag.HasFlag(ModFlag.Mirror))
            icons[7].gameObject.SetActive(true);
    }
}
