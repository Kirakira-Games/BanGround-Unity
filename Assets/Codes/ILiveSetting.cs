using System.Collections.Generic;
using UniRx.Async;

public interface ILiveSetting
{
    string assetDirectory { get; }
    List<ModBase> attachedMods { get; set; }
    DemoFile DemoFile { get; set; }
    string IconPath { get; }
    int NoteScreenTime { get; }
    float SpeedCompensationSum { get; set; }

    bool AddMod(ModBase mod);
    void RemoveAllMods();
    void RemoveMod(ModBase mod);
}