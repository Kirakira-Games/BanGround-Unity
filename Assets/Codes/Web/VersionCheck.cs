using UnityEngine;
using Cysharp.Threading.Tasks;

public class VersionCheck
{
    public static string CheckUpdate => "VersionCheck.CheckingUpdate".L();
    public static string CheckError => "VersionCheck.ErrorGettingInfo".L();
    public static string UpdateForce => "VersionCheck.HasNewForceUpdate".L();
    public static string UpdateNotForce => "VersionCheck.HasNewUpdate".L();
    public static string NoUpdate => "VersionCheck.UpToDate".L();

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
