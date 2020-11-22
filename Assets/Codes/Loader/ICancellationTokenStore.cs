using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public interface ICancellationTokenStore
{
    CancellationToken sceneToken { get; }
    CancellationTokenSource sceneTokenSource { get; }
}
