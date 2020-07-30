using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UniRx.Async;
using BanGround.API;
using System;
using System.Net;

public class Authenticate
{
    private readonly APIMain api = new APIMain("https://banground.herokuapp.com", "zh-CN");

    public static User user = null;

    static KVarRef cl_accessToken = new KVarRef("cl_accesstoken");
    static KVarRef cl_refreshToken = new KVarRef("cl_refreshtoken");

    public static bool isNetworkError;
    public static bool isAuthing = false;

    public async UniTask<bool> TryAuthenticate()
    {
        if (string.IsNullOrEmpty(cl_accessToken) || string.IsNullOrEmpty(cl_refreshToken))
            throw new InvalidOperationException("You can not use this while tokens are empty!");

        LoadingBlocker.instance.Show("Loging in...");

        try
        {
            isAuthing = true;

            api.AccessToken = cl_accessToken;

            var (result, code) = await api.A<RefreshAccessTokenArgs, UserAuth>()(new RefreshAccessTokenArgs
            {
                RefreshToken = cl_refreshToken,
            });

            if(code >= 200 && code < 300)
            {
                user = result.Data.User;

                cl_accessToken.Set(result.Data.AccessToken);
                cl_refreshToken.Set(result.Data.RefreshToken);

                api.AccessToken = result.Data.AccessToken;
            }
            else if(code >= 400 && code < 500)
            {
                cl_accessToken.Set("");
                cl_refreshToken.Set("");
            }
            else if(code >= 500)
            {
                throw new WebException("server says i exploded.");
            }

            isAuthing = false;

            LoadingBlocker.instance.Close();

            return result == null ? false : result.Status;
        }
        catch (Exception ex)
        {
            LoadingBlocker.instance.Close();

            isAuthing = false;
            isNetworkError = true;
            Debug.LogError(ex.Message);

            user = new User
            {
                Avatar = "N/A",
                Nickname = "Offline",
                Username = "Offline"
            };

            return false;
        }
    }

    public async UniTask<bool> TryAuthenticate(string username, string password, bool encryped = true)
    {
        LoadingBlocker.instance.Show("Loging in...");

        if (!encryped)
            password = Auth.EncryptPassword(password);

        try
        {
            isAuthing = true;

            var (result, code) = await api.A<LoginArgs, UserAuth>()(new LoginArgs
            {
                Account = username,
                Password = password
            });

            if (result.Status)
            {
                user = result.Data.User;

                cl_accessToken.Set(result.Data.AccessToken);
                cl_refreshToken.Set(result.Data.RefreshToken);

                api.AccessToken = result.Data.AccessToken;
            }

            isAuthing = false;

            LoadingBlocker.instance.Close();

            return result.Status;
        }
        catch(Exception ex)
        {
            LoadingBlocker.instance.Close();

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