using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;

namespace BanGround.API
{
    public enum HttpRequestMethod
    {
        Get,
        Post
    }

    public class APIMain
    {
        public Dictionary<(Type, Type), string> APIList = new Dictionary<(Type, Type), string>
        {
            { (typeof(LoginArgs), typeof(UserAuth)), "/api/auth/login"},
            { (typeof(RegisterArgs), typeof(UserAuth)), "/api/auth/register"},
            { (typeof(RefreshAccessTokenArgs), typeof(UserAuth)), "/api/auth/refresh-access-token"},
            { (typeof(string), typeof(SendVerificationEmailArgs)), "/api/auth/send-verification-email"},
        };

        public Func<T1, Result<T2>> S<T1,T2>()
        {
            var link = APIList[(typeof(T1), typeof(T2))];

            return (T1 args) =>
            {
                return DoRequest<T2>(link, HttpRequestMethod.Post, args.ToString());
            };
        }

        public Func<T1, UniTask<Result<T2>>> A<T1, T2>()
        {
            var link = APIList[(typeof(T1), typeof(T2))];

            return async (T1 args) =>
            {
                return await DoRequestAsync<T2>(link, HttpRequestMethod.Post, args.ToString());
            };
        }

        internal readonly string Root;
        private readonly string Language;

        public APIMain(string backendAddress, string language)
        {
            Root = backendAddress;
            Language = language;
        }

        string UA = "BanGround-Unity/Alpha (" + 
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

        public string AccessToken = "";

        public Result<T> DoRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.Get, string postJson = null)
        {
            var asyncTask = DoRequestAsync<T>(url, method, postJson);

            asyncTask.RunSynchronously();

            return asyncTask.Result;
        }

        public async Task<Result<T>> DoRequestAsync<T>(string url, HttpRequestMethod method = HttpRequestMethod.Get, string postJson = "{}")
        {
            var uri = new Uri(Root + url);

            string m = "GET";

            if (method == HttpRequestMethod.Post)
                m = "POST";

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, m))
            {
                webRequest.SetRequestHeader("User-Agent", UA);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", AccessToken);
                webRequest.SetRequestHeader("Accpet-Language", Language);

                byte[] jsonToSend = new UTF8Encoding().GetBytes(postJson);

                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                await webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    throw new WebException("Network error!");
                }

                return JsonConvert.DeserializeObject<Result<T>>(webRequest.downloadHandler.text);
            }
        }
    }

    public class User
    {
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("nickname")]
        public string Nickname;

        [JsonProperty("avatar")]
        public string Avatar;

        [JsonProperty("group")]
        public string Group;
    }

    public class Result<T>
    {
        [JsonProperty("status")]
        public bool Status = false;

        [JsonProperty("error")]
        public string Error;

        [JsonProperty("data")]
        public T Data;
    }
}