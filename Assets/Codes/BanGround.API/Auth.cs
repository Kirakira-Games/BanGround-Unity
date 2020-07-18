using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BanGround.API
{
    public class Auth
    {
        // From https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
        private static unsafe class HexUtil
        {
            private static readonly uint[] _lookup32Unsafe = CreateLookup32Unsafe();
            private static readonly uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_lookup32Unsafe, GCHandleType.Pinned).AddrOfPinnedObject();

            private static uint[] CreateLookup32Unsafe()
            {
                var result = new uint[256];
                for (int i = 0; i < 256; i++)
                {
                    string s = i.ToString("x2");
                    if (BitConverter.IsLittleEndian)
                        result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
                    else
                        result[i] = ((uint)s[1]) + ((uint)s[0] << 16);
                }
                return result;
            }

            public static string ByteArrayToHexViaLookup32Unsafe(byte[] bytes)
            {
                var lookupP = _lookup32UnsafeP;
                var result = new char[bytes.Length * 2];
                fixed (byte* bytesP = bytes)
                fixed (char* resultP = result)
                {
                    uint* resultP2 = (uint*)resultP;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        resultP2[i] = lookupP[bytesP[i]];
                    }
                }
                return new string(result);
            }
        }

        public static string EncryptPassword(string raw) => HexUtil.ByteArrayToHexViaLookup32Unsafe(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(raw + "@BanGround")));
    }

    public class LoginArgs
    {
        [JsonProperty("account")]
        public string Account;
        [JsonProperty("password")]
        public string Password;

        public override string ToString() => JsonConvert.SerializeObject(this);
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

        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public class RefreshAccessTokenArgs
    {
        [JsonProperty("refreshToken")]
        public string RefreshToken;

        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public class SendVerificationEmailArgs
    {
        [JsonProperty("email")]
        public string Email;
    }
}
