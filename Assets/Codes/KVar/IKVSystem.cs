using System;
using System.Collections.Generic;

public interface IKVSystem : IEnumerable<KonCommandBase>
{
    bool CanCheat { get; }
    void ExecuteFile(string file);
    void ExecuteLine(string line, bool userInput = false);
    void ReloadConfig();
    void SaveConfig();
    void Add(KonCommandBase cmd);
    KVar Find(string name);
    void WhenRemoteConfigLoaded(Action callback);
}
