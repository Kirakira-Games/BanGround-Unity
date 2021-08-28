using BanGround.Game.Mods;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ModPanel : MonoBehaviour
{
    [Header("Mod Toggles")]
    public Toggle AutoPlay;
    public StepToggle SpeedDown;
    public StepToggle SpeedUp;
    public Toggle SuddenDeath;
    public Toggle Perfect;
    public Toggle Mirror;

    public void Refresh(ModFlag flag)
    {
        AutoPlay.isOn = flag.HasFlag(ModFlag.AutoPlay);
        SpeedDown.SetStep(flag);
        SpeedUp.SetStep(flag);
        SuddenDeath.isOn = flag.HasFlag(ModFlag.SuddenDeath);
        Perfect.isOn = flag.HasFlag(ModFlag.Perfect);
        Mirror.isOn = flag.HasFlag(ModFlag.Mirror);
    }

    public ModFlag GetCurrentFlag()
    {
        ModFlag flag = ModFlag.None;
        flag |= SpeedUp.GetStep();
        flag |= SpeedDown.GetStep();

        if (SuddenDeath.isOn)
            flag |= ModFlag.SuddenDeath;

        if (Perfect.isOn)
            flag |= ModFlag.Perfect;

        if (AutoPlay.isOn)
            flag |= ModFlag.AutoPlay;

        if (Mirror.isOn)
            flag |= ModFlag.Mirror;

        return flag;
    }

    public void Save(KVar kModFlag)
    {
        kModFlag.SetMod(GetCurrentFlag());
    }
}