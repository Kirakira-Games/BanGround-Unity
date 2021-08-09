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

    private const string Prefix = "https://api.reikohaku.fun/api";
    private readonly string API = "/version/check?version=" + Application.version;

    // TODO：鲨了就行
    public async UniTask<VersionData> GetVersionInfo()
    {
        string FullAPI = Prefix + API;

        try
        {
            var req = kiraWebRequest.New<VersionData>()
                    .SetTimeout(2000)
                    .SetIsFullAddress(true)
                    .Get(FullAPI);

            return await req.Fetch();
        }
        catch
        {
            return null;
        }
    }
}

public class VersionData
{
    public int id;
    public string version;
    public bool force;
    //public bool has;
}
