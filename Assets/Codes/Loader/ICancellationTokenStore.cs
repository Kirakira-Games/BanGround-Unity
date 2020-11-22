using System.Threading;

public interface ICancellationTokenStore
{
    CancellationToken sceneToken { get; }
    CancellationTokenSource sceneTokenSource { get; }
}
