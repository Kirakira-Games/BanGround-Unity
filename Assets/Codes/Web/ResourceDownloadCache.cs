using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private ICancellationTokenStore cancellationTokenStore;

        [Inject]
        private void Inject(ICancellationTokenStore store)
        {
            cancellationTokenStore = store;
            WaitReleaseTextures().Forget();
        }

        private async UniTaskVoid WaitReleaseTextures()
        {
            // 等待TokenStore换token
            await UniTask.DelayFrame(1);
            await cancellationTokenStore.sceneToken.WaitUntilCanceled();
            var keys = Keys.ToArray();
            foreach (var key in keys)
            {
                var promise = this[key];
                Remove(key);
                if (promise.isDone && promise.webRequest.result == UnityWebRequest.Result.Success)
                    GameObject.Destroy(((DownloadHandlerTexture)promise.webRequest.downloadHandler).texture);
            }
        }

        /// <summary>
        /// Fetch texture from cache or create a new <see cref="UnityWebRequest"/>
        /// </summary>
        /// <param name="url">Url to download from.</param>
        /// <returns>Texture2D</returns>
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
                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    Remove(url);
                    return null;
                }
                var tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                if (string.IsNullOrEmpty(tex.name))
                {
                    tex.name = url;
                    tex.Compress(false);
                }
                return tex;
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