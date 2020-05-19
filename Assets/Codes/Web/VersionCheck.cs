using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class VersionCheck
{
    public const string CheckUpdate = "正在检查更新";
    public const string CheckError = "获取更新信息失败，你最好联网获取信息后再进行游戏。";
    public const string UpdateForce = "获取到有新的版本：{0}，你需要更新到最新版才能进行游戏";
    public const string UpdateNotForce = "建议更新到最新版{0}";
    public const string NoUpdate = "当前客户端已经是最新版了";

    public VersionRespone respone;
    public static VersionCheck Instance = new VersionCheck
    {
        respone = new VersionRespone
        {
            result = true,
            data = new VersionData
            {
                version = Application.version
            }
        }
    };

    private const string Prefix = "https://api.reikohaku.fun/api";
    private readonly string API = "/version/check?version=" + Application.version;

    public IEnumerator GetVersionInfo()
    {
        string FullAPI = Prefix + API;
        var req = new KirakiraWebRequest<VersionRespone>();
        yield return req.Get(FullAPI);
        if (req.isNetworkError)
            respone = null;
        else
            respone = req.resp;
    }
}

public class VersionRespone
{
    public bool result;
    public VersionData data;
}
public class VersionData
{
    public int id;
    public string version;
    public bool force;
    //public bool has;
}
