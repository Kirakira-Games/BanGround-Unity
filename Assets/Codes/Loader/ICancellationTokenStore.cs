using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public interface ICancellationTokenStore
{
    CancellationToken sceneToken { get; }
    CancellationTokenSource sceneTokenSource { get; }
}

namespace BanGround.Utils
{
    public static class UniTaskExtension
    {
        private static void LogCancellation(OperationCanceledException e)
        {
            Debug.Log(e.Message);
        }

        public static async UniTask IgnoreCancellation(this UniTask task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                LogCancellation(e);
            }
            return;
        }

        public static async UniTask<T> IgnoreCancellation<T>(this UniTask<T> task)
        {
            try
            {
                return await task;
            }
            catch (OperationCanceledException e)
            {
                LogCancellation(e);
            }
            return default;
        }
    }
}