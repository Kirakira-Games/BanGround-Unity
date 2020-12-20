using System.Collections.Generic;

namespace BanGround.Game.Mods
{
    public interface IModManager
    {
        List<ModBase> AttachedMods { get; }
        float SpeedCompensationSum { get; }
        int NoteScreenTime { get; }
        ModFlag Flag { get; }
        float ScoreMultiplier { get; }
    }
}
