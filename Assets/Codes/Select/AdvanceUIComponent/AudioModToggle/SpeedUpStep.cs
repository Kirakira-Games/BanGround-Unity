using BanGround.Game.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpeedUpStep : StepToggle
{
    public override ModFlag GetStep()
    {
        if (index == 1) return ModFlag.Double;
        else if (index == 2) return ModFlag.NightCore;
        else return ModFlag.None;
    }

    public override void SetStep(ModFlag mods)
    {
        if (mods.HasFlag(ModFlag.Double))
        {
            index = 1;
        }
        else if (mods.HasFlag(ModFlag.NightCore))
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