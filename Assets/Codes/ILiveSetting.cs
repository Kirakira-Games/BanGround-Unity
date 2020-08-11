using System.Collections.Generic;
using UniRx.Async;

public interface ILiveSetting
{
    int actualDifficulty { get; set; }
    string assetDirectory { get; }
    List<ModBase> attachedMods { get; set; }
    cHeader CurrentHeader { get; }
    DemoFile DemoFile { get; set; }
    string IconPath { get; }
    int NoteScreenTime { get; }
    bool offsetAdjustMode { get; set; }
    float SpeedCompensationSum { get; set; }
    GameChartData gameChart { get; }
    int currentChart { get; set; }
    V2.Chart chart { get; }

    bool AddMod(ModBase mod);
    UniTask<bool> LoadChart(bool convertToGameChart);
    void RemoveAllMods();
    void RemoveMod(ModBase mod);
}