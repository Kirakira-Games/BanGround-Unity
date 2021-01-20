using UnityEngine;
using BanGround.Web;
using Cysharp.Threading.Tasks;
using Zenject;

public class VersionCheck
{
    [Inject]
    IKiraWebRequest kiraWebRequest;
    
    public static string CheckUpdate => "VersionCheck.CheckingUpdate".L();
    public static string CheckError => "VersionCheck.ErrorGettingInfo".L();
    public static string UpdateForce => "VersionCheck.HasNewForceUpdate".L();
    public static string UpdateNotForce => "VersionCheck.HasNewUpdate".L();
    public static string NoUpdate => "VersionCheck.UpToDate".L();

    public VersionResponse response;
    
    [Inject]
    public void Inject()
    {
        response = new VersionResponse
        {
            result = true,
            data = new VersionData
            {
                version = Application.version
            }
        };
    }

    private const string Prefix = "https://api.reikohaku.fun/api";
    private readonly string API = "/version/check?version=" + Application.version;

    // TODO：鲨了就行
    public async UniTask<VersionResponse> GetVersionInfo()
    {
        string FullAPI = Prefix + API;

        var req = kiraWebRequest.New<VersionResponse>()
                .SetTimeout(2000)
                .SetIsFullAddress(true)
                .Get(FullAPI);

        return await req.Fetch();
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
