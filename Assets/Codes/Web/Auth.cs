using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UniRx.Async;

namespace Web.Auth
{
    public static class Util
    {
        private static string ToHex(byte[] bytes, bool upperCase = false)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        public static string EncryptPassword(string raw) => ToHex(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(raw + "@BanGround")));
    }

    public class User
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("nickname")]
        public string Nickname;

        [JsonProperty("avatar")]
        public string Avatar;

        [JsonProperty("group")]
        public string Group;
    }

    public class LoginArgs
    {
        [JsonProperty("account")]
        public string Account;
        [JsonProperty("password")]
        public string Password;
    }

    public class UserAuth
    {
        [JsonProperty("user")]
        public User User;

        [JsonProperty("refreshToken")]
        public string RefreshToken;

        [JsonProperty("accessToken")]
        public string AccessToken;
    }

    public class RegisterArgs
    {
        [JsonProperty("username")]
        public string Username;
        [JsonProperty("password")]
        public string Password;
        [JsonProperty("nickname")]
        public string Nickname;
        [JsonProperty("email")]
        public string Email;
        [JsonProperty("code")]
        public string Code;
        [JsonProperty("token")]
        public string Token;
        [JsonProperty("cdk")]
        public string Key;
    }

    public class RefreshAccessTokenArgs
    {
        [JsonProperty("refreshToken")]
        public string RefreshToken;
    }

    public class SendVerificationEmailArgs
    {
        [JsonProperty("email")]
        public string Email;
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder RefreshAccessToken(this IKiraWebRequest web, string refreshToken)
        {
            return web.New().SetReq(new RefreshAccessTokenArgs { RefreshToken = refreshToken }).Post("auth/refresh-access-token");
        }

        public static void UpdateUserInfo(this IKiraWebRequest web, UserAuth user)
        {
            UserInfo.user = user.User;
            web.AccessToken = user.AccessToken;
            web.RefreshToken = user.RefreshToken;
        }

        public static async UniTask<UserAuth> DoRefreshAccessToken(this IKiraWebRequest web)
        {
            var user = await web.RefreshAccessToken(web.RefreshToken).Fetch<UserAuth>();
            web.UpdateUserInfo(user);
            return user;
        }

        public static KiraWebRequest.Builder Login(this IKiraWebRequest web, string account, string password)
        {
            password = Util.EncryptPassword(password);
            return web.New().SetReq(new LoginArgs { Account = account, Password = password }).Post("auth/login");
        }

        public static async UniTask<UserAuth> DoLogin(this IKiraWebRequest web, string account, string password)
        {
            var user = await web.Login(account, password).Fetch<UserAuth>();
            web.UpdateUserInfo(user);
            return user;
        }
    }
}