using System.Collections.Generic;

public interface IModManager
{
    List<ModBase> attachedMods { get; }
    bool isSuppressingMods { get; }
    bool isAutoplay { get; }
    float SpeedCompensationSum { get; }
    int NoteScreenTime { get; }
    ulong Flag { get; }
    float ScoreMultiplier { get; }

    bool AddMod(ModBase mod);
    void RemoveAllMods();
    void RemoveMod(ModBase mod);
    void SuppressAllMods(bool suppress);
}