using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

public class CancellationTokenStore : ICancellationTokenStore
{
    public CancellationTokenSource sceneTokenSource { get; private set; } = new CancellationTokenSource();
    public CancellationToken sceneToken => sceneTokenSource.Token;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneTokenSource.Cancel();
        sceneTokenSource.Dispose();
        sceneTokenSource = new CancellationTokenSource();
    }

    public CancellationTokenStore()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}