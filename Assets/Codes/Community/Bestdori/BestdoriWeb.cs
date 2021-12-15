using BanGround.Web;
using BanGround.Web.Chart;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Events;
using System.Collections.Generic;

namespace BanGround.Community
{
    public class BestdoriResult
    {
        public bool result = false;
        public string code;
    }

    public class BestdoriChartListRequest
    {
        public string categoryId = "chart";
        public string categoryName = "SELF_POST";
        public bool following = false;
        public int limit;
        public int offset;
        public string order = "TIME_DESC";
        public string search;
        public BestdoriChartListRequest(int offset, int limit, string search = null)
        {
            this.offset = offset;
            this.limit = limit;
            this.search = search;
        }
    }

    public class BestdoriChart
    {
        public class Song
        {
            public int id;
            public string type;
            public string audio;
            public string cover;

            public string BackgroundUrl
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(cover))
                    {
                        return cover;
                    }
                    // TODO: check bandori official background and chart
                    return null;
                }
            }
        }

        public class Author
        {
            public string username;
            public string nickname;

            public UserItem ToUserItem()
            {
                return new UserItem
                {
                    Username = username,
                    Nickname = nickname
                };
            }
        }

        public class Note
        {
            public int lane;
            public float beat;
            public float bpm;
            public float width;
            public string type;
            public string direction;
            public bool flick;
            public bool skill;
            public List<Note> connections;
        }

        public int id;
        public string title;
        public int diff;
        public int level;
        public Song song;
        public List<Note> chart;
        public Author author;
        public string artists;

        public ChartItem ToChartItem()
        {
            List<int> difficulty = new List<int>(5);
            difficulty[Mathf.Clamp(diff, 0, 4)] = level;
            return new ChartItem
            {
                Id = id,
                Source = ChartSource.Burrito,
                Difficulty = difficulty,
                Uploader = author?.ToUserItem() ?? UserItem.Anonymous,
                BackgroundUrl = song?.BackgroundUrl,
            };
        }
    }

    public class BestdoriChartListResponse : BestdoriResult
    {
        public int count;
        public List<BestdoriChart> posts;
    }

    public class BestdoriChartInfoResponse : BestdoriResult
    {
        public BestdoriChart post;
    }

    public class BestdoriWebRequest : KiraWebRequest
    {
        public override string ServerAddr => "https://bestdori.com/api/";

        public BestdoriBuilder<ResponseType> BestdoriNew<ResponseType>() where ResponseType : BestdoriResult
        {
            return new BestdoriBuilder<ResponseType> { context = this };
        }

        public BestdoriBuilder<BestdoriChartListResponse> GetAllCharts(int offset = 0, int limit = 20, string search = null)
        {
            return BestdoriNew<BestdoriChartListResponse>()
                .SetReq(new BestdoriChartListRequest(offset, limit, search))
                .Post($"post/list");
        }

        public BestdoriBuilder<BestdoriChartInfoResponse> GetChartById(int id)
        {
            return BestdoriNew<BestdoriChartInfoResponse>()
                .Post($"post/details?id={id}");
        }

        public class BestdoriBuilder<ResponseType> where ResponseType : BestdoriResult
        {
            public IKiraWebRequest context;

            public UnityWebRequest webRequest { get; private set; }
            public byte[] request { get; private set; }
            public bool isAborted { get; private set; } = false;
            public string contentType { get; private set; }
            public string url { get; private set; }
            public string method { get; private set; }
            public int timeout { get; private set; } = 60;
            public bool fullAddr { get; private set; } = false;

            protected virtual Resp HandleResponse<Resp>() where Resp : BestdoriResult
            {
                if (isAborted)
                    throw new KiraUserAbortedException();
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    webRequest.responseCode >= 500)
                    throw new KiraNetworkErrorException();
                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    var err = new KiraErrorMessage();
                    err.error = Enum.GetName(typeof(KiraHttpCode), webRequest.responseCode);
                    throw new KiraWebException(webRequest.responseCode, err);
                }
                if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    Debug.Log("[BWR] Response: " + webRequest.downloadHandler.text);
                    var text = webRequest.downloadHandler.text;

                    var resp = JsonConvert.DeserializeObject<Resp>(text);
                    if (resp.result == false)
                        throw new KiraWebException(webRequest.responseCode, new KiraErrorMessage(resp.code));
                    return JsonConvert.DeserializeObject<Resp>(text);
                }
                Debug.Log("[BWR] Success without response text.");
                return default;
            }

            private void Create()
            {
                var addr = fullAddr ? url : context.ServerAddr + url;

                webRequest = new UnityWebRequest(addr, method);
                webRequest.timeout = timeout;
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("User-Agent", context.UA);
                webRequest.SetRequestHeader("Accpet-Language", context.Language);
                if (request != null)
                    webRequest.uploadHandler = new UploadHandlerRaw(request);
                if (contentType != null)
                    webRequest.SetRequestHeader("Content-Type", contentType);
            }

            public BestdoriBuilder<ResponseType> SetReq<Req>(Req req)
            {
                var text = JsonConvert.SerializeObject(req);
                Debug.Log("[BWR] Request: " + text);
                request = new UTF8Encoding().GetBytes(text);
                contentType = "application/json";
                return this;
            }

            public BestdoriBuilder<ResponseType> SetTimeout(int timeout)
            {
                this.timeout = timeout;
                return this;
            }

            public BestdoriBuilder<ResponseType> SetIsFullAddress(bool isFullAddr)
            {
                fullAddr = isFullAddr;
                return this;
            }

            public BestdoriBuilder<ResponseType> Get(string url)
            {
                this.url = url;
                method = "GET";
                Create();
                return this;
            }

            public BestdoriBuilder<ResponseType> Post(string url)
            {
                this.url = url;
                method = "POST";
                Create();
                return this;
            }

            public BestdoriBuilder<ResponseType> AbortOn(UnityAction action)
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

            public async UniTask<T> Fetch<T>() where T : BestdoriResult
            {
                if (isAborted)
                    return default;
                try
                {
                    await webRequest.SendWebRequest();
                }
                catch { }
                return HandleResponse<T>();
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
                await Fetch<BestdoriResult>();
            }
        }
    }
}
