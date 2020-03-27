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

    public VersionInfo version;
    public static VersionCheck Instance = new VersionCheck
    {
        version = new VersionInfo
        {
            status = true,
            data = new VersionData
            {
                has = false
            }
        }
    };

    private const string Prefix = "https://tempapi.banground.fun";
    private readonly string API = "/update/" + Application.version;

    public IEnumerator GetVersionInfo()
    {
        string FullAPI = Prefix + API;
        var req = new KirakiraWebRequest<VersionInfo>();
        yield return req.Get(FullAPI);
        if (req.isNetworkError)
            version = null;
        else
            version = req.resp;
    }
}

public class VersionInfo
{
    public bool status;
    public VersionData data;
}
public class VersionData
{
    public string version;
    public bool force;
    public bool has;
}
