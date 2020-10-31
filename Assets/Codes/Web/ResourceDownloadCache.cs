using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace BanGround.Web
{
    public interface IResourceDownloadCache<T> : IDictionary<string, UnityWebRequestAsyncOperation>
    {
        UniTask<T> Fetch(string url);
    }

    public class TextureDownloadCache : Dictionary<string, UnityWebRequestAsyncOperation>, IResourceDownloadCache<Texture2D>
    {
        [Inject]
        private ICancellationTokenStore cancellationTokenStore;

        /// <summary>
        /// Fetch from cache or create a new <see cref="UnityWebRequest"/>
        /// </summary>
        /// <param name="request">Instance of the UnityWebRequest.</param>
        /// <returns></returns>
        public async UniTask<Texture2D> Fetch(string url)
        {
            UnityWebRequest request = null;
            try
            {
                if (TryGetValue(url, out var req))
                {
                    await UniTask.WaitUntil(() => req.isDone);
                    request = req.webRequest;
                }
                else
                {
                    request = UnityWebRequestTexture.GetTexture(url);
                    var asyncOp = request.SendWebRequest();
                    Add(url, asyncOp);
                    await asyncOp.WithCancellation(cancellationTokenStore.sceneToken);
                }
                if (request.isHttpError || request.isNetworkError)
                {
                    request.Dispose();
                    Remove(url);
                    return null;
                }
                return ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Download cancelled: " + url);
                request?.Dispose();
                Remove(url);
                throw e;
            }
        }
    }
}