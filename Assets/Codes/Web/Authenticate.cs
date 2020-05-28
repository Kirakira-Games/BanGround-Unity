using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UniRx.Async;

public class Authenticate
{
    private const string Prefix = "https://api.reikohaku.fun/api";
    private const string API = "/auth/check";
    private const string FullAPI = Prefix + API;

    public static bool isNetworkError;

    public static async UniTask<AuthResponse> TryAuthenticate(string key, string uuid)
    {
        var req = new KirakiraWebRequest<AuthResponse>();
        var body = new AuthRequest { key = key, uuid = uuid };
        await req.Post(FullAPI, body);
        isNetworkError = req.isNetworkError;
        if (req.isNetworkError)
        {
            return new AuthResponse
            {
                status = false,
                error = "Network error"
            };
        }
        else
        {
            return req.resp;
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

