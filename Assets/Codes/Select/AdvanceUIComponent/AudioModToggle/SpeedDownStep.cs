using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpeedDownStep : StepToggle
{
    public override AudioMod GetStep()
    {
        if (index == 1) return HalfMod.Instanse;
        else if (index == 2) return DayCoreMod.Instanse;
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
                    if (mod is HalfMod) index = 1;
                    else if (mod is DayCoreMod) index = 2;
                    else index = 0;
                    break;
                }
            }
        }
        OnIndexChanged();
    }

}
