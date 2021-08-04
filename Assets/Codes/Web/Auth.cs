using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

namespace BanGround.Web.Auth
{
    public static class Util
    {
        public static string ToHex(byte[] bytes, bool upperCase = false)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        public static string EncryptPassword(string raw) => ToHex(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(raw + "@BanGround")));
    }

    public class UserBadge
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("desc")]
        public string Description;

        [JsonProperty("image")]
        public string ImageUrl;

        [JsonProperty("on")]
        public bool IsOn;
    }

    public class BandLite
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;
    }

    public class UserFull
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("nickname")]
        public string Nickname;

        [JsonProperty("avatar")]
        public string Avatar;

        [JsonProperty("sign")]
        public string Sign;

        [JsonProperty("sex")]
        public int Sex;

        [JsonProperty("exp")]
        public int Experience;

        [JsonProperty("group")]
        public string Group;

        [JsonProperty("badge")]
        public UserBadge[] Badge;

        [JsonProperty("fish")]
        private string _fish;

        [JsonIgnore]
        private long _fishCache = long.MinValue;

        [JsonIgnore]
        public long Fish => _fishCache == long.MinValue ? _fishCache = long.Parse(_fish) : _fishCache;

        [JsonProperty("isLeader")]
        public bool IsLeader;

        [JsonProperty("band")]
        public BandLite Band;

        [JsonProperty("supporter")]
        public int SupporterLevel;

        [JsonIgnore]
        private Texture2D _avatarCache;

        public async UniTask<Texture2D> GetAvatarTexure()
        {
            if (_avatarCache == null && Avatar != null)
            {
                using (WebClient wc = new WebClient())
                {
                    var data = await wc.DownloadDataTaskAsync(new System.Uri(Avatar));

                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    tex.wrapMode = TextureWrapMode.Clamp;

                    _avatarCache = tex;
                }
            }

            return _avatarCache;
        }
    }
    public class UserLite
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
        public UserLite User;

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
        public static KiraWebRequest.Builder<UserAuth> RefreshAccessToken(this IKiraWebRequest web, string refreshToken)
        {
            return web.New<UserAuth>().SetReq(new RefreshAccessTokenArgs { RefreshToken = refreshToken }).Post("auth/refresh-access-token");
        }

        public static void UpdateTokens(this IKiraWebRequest web, UserAuth user)
        {
            web.AccessToken = user.AccessToken;
            web.RefreshToken = user.RefreshToken;
        }

        public static async UniTask<UserAuth> DoRefreshAccessToken(this IKiraWebRequest web)
        {
            var user = await web.RefreshAccessToken(web.RefreshToken).Fetch();
            web.UpdateTokens(user);
            return user;
        }

        public static KiraWebRequest.Builder<UserAuth> Login(this IKiraWebRequest web, string account, string password)
        {
            password = Util.EncryptPassword(password);
            return web.New<UserAuth>().SetReq(new LoginArgs { Account = account, Password = password }).Post("auth/login");
        }

        public static async UniTask<UserAuth> DoLogin(this IKiraWebRequest web, string account, string password)
        {
            var user = await web.Login(account, password).Fetch();
            web.UpdateTokens(user);
            return user;
        }
    }
}