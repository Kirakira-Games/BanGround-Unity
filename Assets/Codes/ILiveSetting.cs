using System.Collections.Generic;
using UniRx.Async;

public interface ILiveSetting
{
    string assetDirectory { get; }
    DemoFile DemoFile { get; set; }
    string IconPath { get; }
}