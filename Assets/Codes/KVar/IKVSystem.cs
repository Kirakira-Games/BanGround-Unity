using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IKVSystem : IEnumerable<KonCommandBase>
{
    bool CanCheat { get; }
    void ExecuteFile(string file);
    void ExecuteLine(string line, bool userInput = false);
    Task ReloadConfig();
    void SaveConfig();
    void Add(KonCommandBase cmd);
    KVar Find(string name);
    void OnConfigDone(Action callback);
}
