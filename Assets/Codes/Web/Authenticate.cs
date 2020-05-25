using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class Authenticate
{
    private const string Prefix = "https://api.reikohaku.fun/api";
    private const string API = "/auth/check";
    private const string FullAPI = Prefix + API;

    public static AuthResponse result;
    public static bool isNetworkError;

    public static IEnumerator TryAuthenticate(string key, string uuid)
    {
        var req = new KirakiraWebRequest<AuthResponse>();
        var body = new AuthRequest { key = key, uuid = uuid };
        yield return req.Post(FullAPI, body);
        isNetworkError = req.isNetworkError;
        if (req.isNetworkError)
        {
            result = new AuthResponse
            {
                status = false,
                error = "Network error"
            };
        }
        else
        {
            result = req.resp;
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
    public string username;
    public string error = "Authentication error";
}

