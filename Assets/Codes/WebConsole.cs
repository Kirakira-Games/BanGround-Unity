using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebConsole : MonoBehaviour
{
    StringBuilder fullLog = new StringBuilder(0x10000);

    public string FullLog
    {
        get
        {
            return fullLog.ToString();
        }
    }

    public Action Action
    {
        set
        {
            actionQueue.Enqueue(value);
        }
    }

    Dictionary<string, byte[]> resourceList = new Dictionary<string, byte[]>();

    Action flushKirapacksAction = () =>
    {
        if (DataLoader.LoadAllKiraPackFromInbox())
        {
            if (SceneManager.GetActiveScene().name == "Select")
            {
                SceneManager.LoadScene("Select");
            }
        }
    };

    Queue<Action> actionQueue = new Queue<Action>();
    List<WebSocketInterface> webSockets = new List<WebSocketInterface>();

    HttpServer httpSv = null;

    class WebSocketInterface : WebSocketBehavior
    {
        public WebConsole console;

        protected override void OnOpen()
        {
            base.OnOpen();

            Send(console.FullLog);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            console.Action = () =>
            {
                KVSystem.Instance.ExecuteLine(e.Data, true);
            };
        }

        public void SendLog(string log)
        {
            Send(log);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            console.webSockets.Remove(this);
        }
    }

    void StartHttp()
    {
        httpSv = new HttpServer("http://0.0.0.0:8088");
        httpSv.OnGet += (sender, ctx) =>
        {
            var path = ctx.Request.Url.AbsolutePath;

            if (path == "/")
                path = "/index.html";

            byte[] content = null;

            if (path == "/log")
            {
                content = Encoding.UTF8.GetBytes(fullLog.ToString());
            }
            else if (path == "/airdrop")
            {
                content = Encoding.UTF8.GetBytes("Hey I mean this is a PUT interface");
            }
            else if (path == "/commands")
            {
                ctx.Response.ContentType = "application/json";

                var json = new StringBuilder();
                bool first = true;

                json.Append("[");

                foreach (var item in KVSystem.Instance)
                {
                    if (!first)
                    {
                        json.Append(",");
                    }
                    else
                    {
                        first = false;
                    }

                    json.Append($"{{\"value\":\"{item.Name}\",\"type\":\"{(item is KVar ? "KVar" : "Kommand")}\",\"help\":\"{item.Description}\"}}");
                }

                json.Append("]");

                content = Encoding.UTF8.GetBytes(json.ToString());
            }
            else
            {
                if (resourceList.ContainsKey(path))
                {
                    content = resourceList[path];
                }
                else
                {
                    ctx.Response.StatusCode = 404;

                    content = Encoding.UTF8.GetBytes("404");
                }
            }

            ctx.Response.ContentLength64 = content.Length;

            using (var bw = new BinaryWriter(ctx.Response.OutputStream))
            {
                bw.Write(content);
            }
        };

        httpSv.OnPut += (obj, ctx) =>
        {
            var path = ctx.Request.Url.AbsolutePath;

            if (path == "/airdrop")
            {
                byte[] bytes;
                using (var br = new BinaryReader(ctx.Request.InputStream))
                {
                    bytes = br.ReadBytes((int)ctx.Request.ContentLength64);
                }

                if (!Directory.Exists(DataLoader.InboxDir))
                    Directory.CreateDirectory(DataLoader.InboxDir);

                File.WriteAllBytes(Path.Combine(DataLoader.InboxDir, Guid.NewGuid().ToString("N") + ".kirapack"), bytes);

                if (!actionQueue.Contains(flushKirapacksAction))
                    actionQueue.Enqueue(flushKirapacksAction);
            }
        };

        httpSv.AddWebSocketService<WebSocketInterface>("/websocket", wsi =>
        {
            webSockets.Add(wsi);
            wsi.console = this;
        });

        httpSv.OnPost += (obj, ctx) =>
        {
            var path = ctx.Request.Url.AbsolutePath;

            byte[] content = null;

            if (path == "/redirect")
            {
                string dataStr = null;

                using (var sr = new StreamReader(ctx.Request.InputStream))
                    dataStr = sr.ReadToEnd();

                var data = JToken.Parse(dataStr);

                try
                {
                    var url = data["url"].ToString();
                    var query = data["query"].ToString();

                    using(var wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";

                        try
                        {
                            content = wc.UploadData(url, Encoding.UTF8.GetBytes(query));
                        }
                        catch(WebException webex)
                        {
                            var resp = (HttpWebResponse)webex.Response;

                            using (var br = new BinaryReader(resp.GetResponseStream()))
                                content = br.ReadBytes((int)resp.ContentLength);

                            ctx.Response.ContentLength64 = resp.ContentLength;
                            ctx.Response.ContentType = resp.ContentType;
                        }
                    }
                }
                catch(Exception ex)
                {
                    ctx.Response.StatusCode = 500;
                    content = Encoding.UTF8.GetBytes(ex.Message);
                }
            }
            else
            {
                ctx.Response.StatusCode = 404;

                content = Encoding.UTF8.GetBytes("404");
            }

            ctx.Response.ContentLength64 = content.Length;

            using (var bw = new BinaryWriter(ctx.Response.OutputStream))
            {
                bw.Write(content);
            }
        };

        httpSv.Start();
    }

    void Update()
    {
        while(actionQueue.Count > 0)
            actionQueue.Dequeue()();
    }

    void Awake()
    {
        Application.logMessageReceivedThreaded += Application_logMessageReceived;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        var resources = WebConsoleResource.GetEnumerator();

        while(resources.MoveNext())
        {
            resourceList.Add(resources.Current.Key, Resources.Load<TextAsset>(resources.Current.Value).bytes);
        }

        resourceList.Add("/register", Resources.Load<TextAsset>("register").bytes);

        new Thread(StartHttp).Start();
    }

    private async void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        var len = condition.Length + stackTrace?.Length + 2;

        if (fullLog.Length + len >= fullLog.Capacity)
            fullLog.Clear();

        fullLog.AppendLine(condition);

        if(type == LogType.Exception)
        {
            fullLog.AppendLine(stackTrace);
        }

        if (webSockets.Count == 0)
            return;

        var str = condition + "\n";
        if (type == LogType.Error)
            str += stackTrace + "\n";

        await Task.Run(() =>
            webSockets.All(ws =>
            {
                if(ws.ConnectionState == WebSocketState.Open)
                    ws.SendLog(str);

                return true;
            })
       );
    }

    private void OnApplicationQuit()
    {
        httpSv?.Stop();
    }
}