using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class VersionCheck
{
    public VersionInfo version;

    public const string CurrentVersion = "0.3.3";
    private const string API = "/version/latest";

    private string FullAPI;

    public VersionCheck(string url)
    {
        if (url.EndsWith("/"))
        {
            FullAPI = url.Substring(0, url.Length - 1) + API;
        }
        else
        {
            FullAPI = url + API;
        }
    }

    public IEnumerator GetVersionInfo()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(FullAPI))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError) version = null;
            else version = JsonConvert.DeserializeObject<VersionInfo>(webRequest.downloadHandler.text);
        }
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
    public bool forceUpdate;
}
