using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UniRx.Async;
using BanGround.API;
using System;

public class Authenticate
{
    private readonly APIMain api = new APIMain("https://banground.herokuapp.com", "zh-CN");

    public static User user = null;

    public static string accessToken = "";
    public static string refreshToken = "";

    public static bool isNetworkError;
    public static bool isAuthing = false;

    public async UniTask<bool> TryAuthenticate(string username, string password, bool encryped = true)
    {
        if(!encryped)
            password = Auth.EncryptPassword(password);

        try
        {
            isAuthing = true;

            var result = await api.A<LoginArgs, UserAuth>()(new LoginArgs
            {
                Account = username,
                Password = password
            });

            if (result.Status)
            {
                user = result.Data.User;

                accessToken = result.Data.AccessToken;
                refreshToken = result.Data.RefreshToken;
            }

            isAuthing = false;

            return result.Status;
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);

            isNetworkError = true;
            isAuthing = false;
            return false;
        }
    }
/*
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
    }*/
}