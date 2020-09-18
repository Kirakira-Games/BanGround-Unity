using Boo.Lang;
using Newtonsoft.Json;
using System.Security.Cryptography;
using UniRx.Async;
using UnityEngine;

namespace Web.Upload
{
    public enum UploadType
    {
        Misc,
        Music,
        Chart,
        Image
    }

    public class File
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public Auth.User Uploader;

        [JsonProperty("size")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Size;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("url")]
        public string Url;
    }

    public class FishDelta
    {
        [JsonProperty("fish")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Fish;

        [JsonProperty("required")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Required;
    }

    public class CalcResult : FishDelta
    {
        [JsonProperty("duplicate")]
        public List<bool> Duplicates = new List<bool>();
    }

    public class FileHashSize
    {
        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("size")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Size;
    }

    public static class Util
    {
        public static string Hash(byte[] content)
        {
            return Auth.Util.ToHex(SHA256.Create().ComputeHash(content));
        }
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder CalcUploadCost(this IKiraWebRequest web, List<FileHashSize> files)
        {
            return web.New().SetReq(files).UseTokens().Post("upload/calc");
        }

        public static async UniTask<CalcResult> DoCalcUploadCost(this IKiraWebRequest web, List<FileHashSize> files)
        {
            return await web.CalcUploadCost(files).Fetch<CalcResult>();
        }

        public static KiraWebRequest.Builder ClaimFiles(this IKiraWebRequest web, List<string> hashes)
        {
            return web.New().SetReq(hashes).UseTokens().Post("upload/claim");
        }

        public static async UniTask<FishDelta> DoClaimFiles(this IKiraWebRequest web, List<string> hashes)
        {
            return await web.ClaimFiles(hashes).Fetch<FishDelta>();
        }

        public static KiraWebRequest.Builder UploadFile(this IKiraWebRequest web, UploadType type, byte[] content)
        {
            string typeStr= type.ToString().ToLower();
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", content);
            return web.New().SetForm(form).UseTokens().Post("upload/" + typeStr);
        }

        public static async UniTask<File> DoUploadFile(this IKiraWebRequest web, UploadType type, byte[] content)
        {
            return await web.UploadFile(type, content).Fetch<File>();
        }
    }
}