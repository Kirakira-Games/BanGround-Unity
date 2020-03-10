using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpeedUpStep : StepToggle
{
    public override AudioMod GetStep()
    {
        if (index == 1) return DoubleMod.Instanse;
        else if (index == 2) return NightCoreMod.Instanse;
        else return null;
    }

    public override void SetStep(List<ModBase> mods)
    {
        if (mods == null || mods.Count == 0)
        {
            index = 0;
        }
        else
        {
            foreach (var mod in mods)
            {
                if (mod is AudioMod)
                {
                    if (mod is DoubleMod) index = 1;
                    else if (mod is NightCoreMod) index = 2;
                    else index = 0;
                    break;
                }
            }
        }
        OnIndexChanged();
    }
}