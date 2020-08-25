using UnityEngine;
using UniRx.Async;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Text;
using Zenject;
using UnityEngine.Events;

namespace Web
{
    using Auth;
    using WebSocketSharp;

    internal class Result<T>
    {
        public bool status = false;
        public string error;
        public T data;
    }

    public class KiraErrorMessage
    {
        public readonly bool status;
        public readonly string error;
        public KiraErrorMessage(string error = "")
        {
            status = false;
            this.error = error;
        }
    }

    public class KiraWebException : Exception
    {
        public long RetCode { get; private set; }
        public KiraErrorMessage Msg { get; private set; }
        public KiraWebException(long code, KiraErrorMessage msg)
        {
            RetCode = code;
            Msg = msg;
            Debug.Log($"[KWR] Error: {code}\n{JsonConvert.SerializeObject(msg)}");
        }

        public override string Message => Msg.error;
        public bool isNetworkError => RetCode == -1;
        public bool isCustomError => RetCode == -2;
        public bool isAborted => RetCode == -3;
    }

    public class KiraNetworkErrorException : KiraWebException
    {
        public KiraNetworkErrorException() : base(-1, new KiraErrorMessage("Network error.")) { }
    }

    public class KiraUserAbortedException : KiraWebException
    {
        public KiraUserAbortedException() : base(-3, new KiraErrorMessage("User aborted.")) { }
    }

    public class KiraWebRequest : IKiraWebRequest
    {
        [Inject(Id = "cl_accesstoken")]
        private KVar cl_accesstoken;
        [Inject(Id = "cl_refreshtoken")]
        private KVar cl_refreshtoken;

        public string UA => "BanGround-Unity/Alpha (" +
#if UNITY_EDITOR
            "Editor"
#elif UNITY_ANDROID
            "Android"
#elif UNITY_IOS
            "iOS"
#elif UNITY_STANDALONE
            "Standalone"
#else 
            "DotNet"
#endif
        + "); ApiRequest";
        public string ServerAddr => "https://banground.live/api/";
        public string Language => "zh-CN";
        public string AccessToken
        {
            get => cl_accesstoken.Get<string>();
            set => cl_accesstoken.Set(value);
        }
        public string RefreshToken
        {
            get => cl_refreshtoken.Get<string>();
            set => cl_refreshtoken.Set(value);
        }

        public Builder New()
        {
            return new Builder { context = this };
        }

        public class Builder
        {
            public IKiraWebRequest context;

            public UnityWebRequest webRequest { get; private set; }
            public byte[] request { get; private set; }
            public bool useTokens { get; private set; } = false;
            public bool autoRefresh { get; private set; } = false;
            public bool isAborted { get; private set; } = false;
            public string contentType { get; private set; }
            public string url { get; private set; }
            public string method { get; private set; }

            private async UniTask<Resp> HandleResponse<Resp>(UnityWebRequest req)
            {
                if (isAborted)
                    throw new KiraUserAbortedException();
                if (req.isNetworkError || req.responseCode >= 500)
                    throw new KiraNetworkErrorException();
                if (req.isHttpError)
                {
                    if (req.responseCode == 401 && autoRefresh && context.RefreshToken != null)
                    {
                        await context.DoRefreshAccessToken();
                        webRequest.Dispose();
                        Create();
                        return await Fetch<Resp>();
                    }
                    var err = JsonConvert.DeserializeObject<KiraErrorMessage>(req.downloadHandler.text);
                    throw new KiraWebException(req.responseCode, err);
                }
                if (!req.downloadHandler.text.IsNullOrEmpty())
                {
                    var result = JsonConvert.DeserializeObject<Result<Resp>>(req.downloadHandler.text);
                    if (result.status == false)
                        throw new KiraWebException(req.responseCode, new KiraErrorMessage(result.error));
                    Debug.Log("[KWR] Resp: " + JsonConvert.SerializeObject(result.data));
                    return result.data;
                }
                Debug.Log("[KWR] Success without response text.");
                return default;
            }

            private void Create()
            {
                webRequest = new UnityWebRequest(context.ServerAddr + url, method);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("User-Agent", context.UA);
                webRequest.SetRequestHeader("Accpet-Language", context.Language);
                if (request != null)
                    webRequest.uploadHandler = new UploadHandlerRaw(request);
                if (contentType != null)
                    webRequest.SetRequestHeader("Content-Type", contentType);
                if (useTokens)
                    webRequest.SetRequestHeader("Authorization", $"Bearer {context.AccessToken}");
            }

            public Builder SetReq<Req>(Req req)
            {
                var text = JsonConvert.SerializeObject(req);
                Debug.Log("[KWR] Request: " + text);
                request = new UTF8Encoding().GetBytes(text);
                contentType = "application/json";
                return this;
            }

            public Builder SetForm(WWWForm form)
            {
                request = form.data;
                contentType = "multipart/form-data";
                return this;
            }

            public Builder UseTokens(bool autoRefresh = true)
            {
                useTokens = true;
                this.autoRefresh = autoRefresh;
                return this;
            }

            public Builder Get(string url)
            {
                this.url = url;
                method = "GET";
                Create();
                return this;
            }

            public Builder Post(string url)
            {
                this.url = url;
                method = "POST";
                Create();
                return this;
            }

            public Builder AbortOn(UnityAction action)
            {
                action += () =>
                {
                    if (!isAborted)
                    {
                        isAborted = true;
                        webRequest.Abort();
                    }
                };
                return this;
            }

            public async UniTask<Resp> Fetch<Resp>()
            {
                if (isAborted)
                    return default;
                return await HandleResponse<Resp>(await webRequest.SendWebRequest());
            }

            public async UniTaskVoid Fetch()
            {
                await Fetch<object>();
            }
        }
    }
}