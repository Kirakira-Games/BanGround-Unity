using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebConsole : MonoBehaviour
{
    bool Stop = false;

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
    }

    void StartHttp()
    {
        httpSv = new HttpServer("http://0.0.0.0:8088");
        httpSv.OnGet += (sender, ctx) =>
        {
            var path = ctx.Request.Url.AbsolutePath;

            if (path == "/")
                path = "/index.html";

            if (path == "/log")
            {
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    sw.Write(fullLog.ToString());
                }
            }
            if (path == "/airdrop")
            {
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    sw.WriteLine("Hey I mean this is a PUT interface");
                }
            }
            else
            {
                if (resourceList.ContainsKey(path))
                {
                    var content = resourceList[path];
                    ctx.Response.ContentLength64 = content.Length;

                    using (var bw = new BinaryWriter(ctx.Response.OutputStream))
                    {
                        bw.Write(content);
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 404;

                    using (var sw = new StreamWriter(ctx.Response.OutputStream))
                    {
                        sw.WriteLine("404");
                    }
                }
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

                File.WriteAllBytes(Path.Combine(DataLoader.InboxDir, Guid.NewGuid().ToString("N") + ".kirapack"), bytes);
            }
        };

        httpSv.AddWebSocketService<WebSocketInterface>("/websocket", wsi =>
        {
            webSockets.Add(wsi);
            wsi.console = this;
        });

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

        resourceList.Add("/index.html", Resources.Load<TextAsset>("WebConsole/index").bytes);
        resourceList.Add("/styles/main.css", Resources.Load<TextAsset>("WebConsole/styles/main.css").bytes);

        StartHttp();
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