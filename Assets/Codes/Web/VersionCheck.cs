using UnityEngine;
using Cysharp.Threading.Tasks;

public class VersionCheck
{
    public const string CheckUpdate = "正在检查更新";
    public const string CheckError = "获取更新信息失败，你最好联网获取信息后再进行游戏。";
    public const string UpdateForce = "获取到有新的版本：{0}，你需要更新到最新版才能进行游戏";
    public const string UpdateNotForce = "建议更新到最新版{0}";
    public const string NoUpdate = "当前客户端已经是最新版了";

    public VersionResponse response;
    public static VersionCheck Instance = new VersionCheck
    {
        response = new VersionResponse
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

    public async UniTask<VersionResponse> GetVersionInfo()
    {
        string FullAPI = Prefix + API;
        var req = new KirakiraWebRequest<VersionResponse>();
        return await req.Get(FullAPI);
    }
}

public class VersionResponse
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
