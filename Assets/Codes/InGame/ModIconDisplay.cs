using BanGround.Game.Mods;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#pragma warning disable 0649
public class ModIconDisplay : MonoBehaviour
{
    [Inject(Id = "cl_modflag")]
    private KVar cl_modflag;

    [SerializeField]
    private Image[] icons;

    private void Start()
    {
        //icons = GetComponentsInChildren<Image>(true);
        var flag = ModFlagUtil.From(cl_modflag);
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
        // TODO: Add mirror
    }
}
