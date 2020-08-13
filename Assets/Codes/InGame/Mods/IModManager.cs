using System.Collections.Generic;

public interface IModManager
{
    List<ModBase> attachedMods { get; }
    bool isSuppressingMods { get; }
    float SpeedCompensationSum { get; }
    int NoteScreenTime { get; }

    bool AddMod(ModBase mod);
    void RemoveAllMods();
    void RemoveMod(ModBase mod);
    void SuppressAllMods(bool suppress);
}