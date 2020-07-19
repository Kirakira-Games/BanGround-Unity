using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public Func<T1, Task<Result<T2>>> A<T1, T2>()
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

        private WebClient webClient = new WebClient();

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

        string AccessToken = "";

        public Result<T> DoRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.Get, string postJson = null)
        {
            var asyncTask = DoRequestAsync<T>(url, method, postJson);

            asyncTask.Wait();

            return asyncTask.Result;
        }

        public async Task<Result<T>> DoRequestAsync<T>(string url, HttpRequestMethod method = HttpRequestMethod.Get, string postJson = "{}")
        {
            var uri = new Uri(Root + url);

            webClient.Headers[HttpRequestHeader.UserAgent] = UA;
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            webClient.Headers[HttpRequestHeader.Authorization] = AccessToken;
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = Language;

            webClient.Encoding = Encoding.UTF8;

            string result = null;

            if(method == HttpRequestMethod.Get)
            {
                result = await webClient.DownloadStringTaskAsync(uri.ToString());
            }
            else if(method == HttpRequestMethod.Post)
            {
                if (postJson == null)
                    throw new InvalidOperationException("YOU CAN'T JUST POST WITHOUT PARAMETER, BOI");

                result = webClient.UploadString(uri.ToString(), postJson);
            }
            else
            {
                throw new Exception("?");
            }

            return JsonConvert.DeserializeObject<Result<T>>(result);
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