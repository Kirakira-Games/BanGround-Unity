using BanGround.Game.Mods;

public class SpeedDownStep : StepToggle
{
    public override ModFlag GetStep()
    {
        if (index == 1) return ModFlag.Half;
        else if (index == 2) return ModFlag.DayCore;
        else return ModFlag.None;
    }

    public override void SetStep(ModFlag mods)
    {
        if (mods.HasFlag(ModFlag.Half))
        {
            index = 1;
        }
        else if (mods.HasFlag(ModFlag.DayCore))
        {
            index = 2;
        }
        else
        {
            index = 0;
        }
        OnIndexChanged();
    }
}
