using UnityEngine;
using Cysharp.Threading.Tasks;
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
#if false
        // Use local server
        public string ServerAddr => "http://localhost:8080/api/";
#elif UNITY_EDITOR
        public string ServerAddr => "https://beijing.aliyun.reikohaku.fun/api/";
#else
        public string ServerAddr => "https://banground.live/api/";
#endif
        public string Language => "zh-CN";
        public string AccessToken
        {
            get => cl_accesstoken;
            set => cl_accesstoken.Set(value);
        }
        public string RefreshToken
        {
            get => cl_refreshtoken;
            set => cl_refreshtoken.Set(value);
        }

        public Builder<ResponseType> New<ResponseType>()
        {
            return new Builder<ResponseType> { context = this };
        }

        public Builder<object> New()
        {
            return new Builder<object> { context = this };
        }

        public class Builder<ResponseType>
        {
            public IKiraWebRequest context;

            public WWWForm form { get; private set; }
            public UnityWebRequest webRequest { get; private set; }
            public byte[] request { get; private set; }
            public bool useTokens { get; private set; } = false;
            public bool autoRefresh { get; private set; } = false;
            public bool isAborted { get; private set; } = false;
            public string contentType { get; private set; }
            public string url { get; private set; }
            public string method { get; private set; }
            public int timeout { get; private set; } = 60;

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
                    Debug.Log("[KWR] Response: " + JsonConvert.SerializeObject(result.data));
                    return result.data;
                }
                Debug.Log("[KWR] Success without response text.");
                return default;
            }

            private void Create()
            {
                if (form == null)
                    webRequest = new UnityWebRequest(context.ServerAddr + url, method);
                else
                    webRequest = UnityWebRequest.Post(context.ServerAddr + url, form);
                webRequest.timeout = timeout;
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

            public Builder<ResponseType> SetReq<Req>(Req req)
            {
                if (form != null)
                    throw new InvalidOperationException("Cannot set request on form data!");
                var text = JsonConvert.SerializeObject(req);
                Debug.Log("[KWR] Request: " + text);
                request = new UTF8Encoding().GetBytes(text);
                contentType = "application/json";
                return this;
            }

            public Builder<ResponseType> SetForm(WWWForm form)
            {
                if (request != null)
                    throw new InvalidOperationException("Cannot set form after setting request!");
                this.form = form;
                return this;
            }

            public Builder<ResponseType> UseTokens(bool autoRefresh = true)
            {
                useTokens = true;
                this.autoRefresh = autoRefresh;
                return this;
            }

            public Builder<ResponseType> SetTimeout(int timeout)
            {
                this.timeout = timeout;
                return this;
            }

            public Builder<ResponseType> Get(string url)
            {
                this.url = url;
                method = "GET";
                Create();
                return this;
            }

            public Builder<ResponseType> Post(string url)
            {
                this.url = url;
                method = "POST";
                Create();
                return this;
            }

            public Builder<ResponseType> AbortOn(UnityAction action)
            {
                action += () => Abort();
                return this;
            }

            public bool Abort()
            {
                if (!isAborted)
                {
                    isAborted = true;
                    webRequest.Abort();
                    return true;
                }
                return false;
            }

            public async UniTask<T> Fetch<T>()
            {
                if (isAborted)
                    return default;
                return await HandleResponse<T>(await webRequest.SendWebRequest());
            }

            public async UniTask<ResponseType> Fetch()
            {
                return await Fetch<ResponseType>();
            }

            /// <summary>
            /// Send a request and ignore the response.
            /// </summary>
            public async UniTask Send()
            {
                await Fetch<object>();
            }
        }
    }
}