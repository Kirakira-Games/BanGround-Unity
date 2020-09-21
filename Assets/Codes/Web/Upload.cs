using Newtonsoft.Json;
using System.Collections.Generic;
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

    public static class Util
    {
        public static string Hash(byte[] content)
        {
            return Auth.Util.ToHex(SHA256.Create().ComputeHash(content));
        }
    }

    public class File
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public Auth.UserLite Uploader;

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

    public static class Extension
    {
        public static KiraWebRequest.Builder<CalcResult> CalcUploadCost(this IKiraWebRequest web, List<FileHashSize> files)
        {
            return web.New<CalcResult>().SetReq(files).UseTokens().Post("upload/calc");
        }

        public static KiraWebRequest.Builder<FishDelta> ClaimFiles(this IKiraWebRequest web, List<string> hashes)
        {
            return web.New<FishDelta>().SetReq(hashes).UseTokens().Post("upload/claim");
        }

        public static KiraWebRequest.Builder<File> UploadFile(this IKiraWebRequest web, UploadType type, byte[] content)
        {
            string typeStr= type.ToString().ToLower();
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", content);
            return web.New<File>().SetForm(form).UseTokens().Post("upload/" + typeStr);
        }
    }
}