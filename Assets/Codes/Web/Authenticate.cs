using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class Authenticate
{
    private const string Prefix = "https://tempapi.banground.fun";
    private const string API = "/auth";
    private const string FullAPI = Prefix + API;

    public static AuthResponse result;
    public static bool isNetworkError;

    public static IEnumerator TryAuthenticate(string key, string uuid)
    {
        var req = new AuthRequest { key = key, uuid = uuid };
        var reqJson = JsonConvert.SerializeObject(req);
        using (UnityWebRequest webRequest = new UnityWebRequest(FullAPI, "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding().GetBytes(reqJson);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError)
            {
                result = new AuthResponse
                {
                    status = false,
                    error = "Network error"
                };
                isNetworkError = true;
            }
            else
            {
                result = JsonConvert.DeserializeObject<AuthResponse>(webRequest.downloadHandler.text);
                isNetworkError = false;
            }
        }
    }
}

class AuthRequest
{
    public string key;
    public string uuid;
}

public class AuthResponse
{
    public bool status;
    public string error = "Authentication error";
}

