using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Text;
using Zenject;
using UnityEngine.Events;

namespace BanGround.Web
{
    using Auth;
    using System.Net;
    using WebSocketSharp;

    enum KiraHttpCode
    {
        Continue = 100,
        SwitchingProtocols = 101,
        OK = 200,
        Created = 201,
        Accepted = 202,
        NonAuthoritativeInformation = 203,
        NoContent = 204,
        ResetContent = 205,
        PartialContent = 206,
        Ambiguous = 300,
        MultipleChoices = 300,
        Moved = 301,
        MovedPermanently = 301,
        Found = 302,
        Redirect = 302,
        RedirectMethod = 303,
        SeeOther = 303,
        NotModified = 304,
        UseProxy = 305,
        Unused = 306,
        RedirectKeepVerb = 307,
        TemporaryRedirect = 307,
        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        RequestTimeout = 408,
        Conflict = 409,
        Gone = 410,
        LengthRequired = 411,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        RequestUriTooLong = 414,
        UnsupportedMediaType = 415,
        RequestedRangeNotSatisfiable = 416,
        ExpectationFailed = 417,
        UpgradeRequired = 426,
        TooManyRequests = 429,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        HttpVersionNotSupported = 505
    }

    internal class Result<T>
    {
        public bool status = false;
        public string error;
        public T data;
    }

    public class KiraErrorMessage
    {
        public bool status;
        public string error;
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

        public string UA => $"BanGround-Unity/{Application.version} (" +
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
        public string ServerAddr => "https://banground.reikohaku.fun/api/";
#else
        public string ServerAddr => "https://banground.live/api/";
#endif
        public string ServerSite => ServerAddr.Substring(0, ServerAddr.Length - 5);
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
            public bool fullAddr { get; private set; } = false;

            private async UniTask<Resp> HandleResponse<Resp>()
            {
                if (isAborted)
                    throw new KiraUserAbortedException();
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    webRequest.responseCode >= 500)
                    throw new KiraNetworkErrorException();
                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    if (webRequest.responseCode == 401 && autoRefresh && context.RefreshToken != null)
                    {
                        await context.DoRefreshAccessToken();
                        webRequest.Dispose();
                        Create();
                        return await Fetch<Resp>();
                    }
                    var err = JsonConvert.DeserializeObject<KiraErrorMessage>(webRequest.downloadHandler.text)
                        ?? new KiraErrorMessage();
                    if (string.IsNullOrEmpty(err.error))
                        err.error = Enum.GetName(typeof(KiraHttpCode), webRequest.responseCode);
                    throw new KiraWebException(webRequest.responseCode, err);
                }
                if (!webRequest.downloadHandler.text.IsNullOrEmpty())
                {
                    Debug.Log("[KWR] Response: " + webRequest.downloadHandler.text);
                    var text = webRequest.downloadHandler.text;

                    var result = JsonConvert.DeserializeObject<Result<Resp>>(text);
                    if (result.status == false)
                        throw new KiraWebException(webRequest.responseCode, new KiraErrorMessage(result.error));
                    return result.data;
                }
                Debug.Log("[KWR] Success without response text.");
                return default;
            }

            private void Create()
            {
                var addr = fullAddr ? url : context.ServerAddr + url;

                if (form == null)
                    webRequest = new UnityWebRequest(addr, method);
                else
                    webRequest = UnityWebRequest.Post(addr, form);
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

            public Builder<ResponseType> SetIsFullAddress(bool isFullAddr)
            {
                fullAddr = isFullAddr;
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
                try
                {
                    await webRequest.SendWebRequest();
                }
                catch { }
                return await HandleResponse<T>();
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