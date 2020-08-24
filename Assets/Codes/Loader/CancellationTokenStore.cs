using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

public class CancellationTokenStore : ICancellationTokenStore
{
    public CancellationTokenSource sceneTokenSource { get; private set; } = new CancellationTokenSource();
    public CancellationToken sceneToken => sceneTokenSource.Token;

    private void OnSceneUnloaded(Scene scene)
    {
        sceneTokenSource.Cancel();
        sceneTokenSource.Dispose();
        sceneTokenSource = new CancellationTokenSource();
    }

    public CancellationTokenStore()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
}