using BanGround.Web.Auth;
using Newtonsoft.Json;

namespace BanGround.Web.File
{
    public class FilenameHash
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("hash")]
        public string Hash;
    }

    public class FileInfo
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public UserLite Uploader;

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

    public class FileDownloadInfo
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("file")]
        public FileInfo File;
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder<FileInfo> GetFileByIdOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New<FileInfo>().Get($"file/{idOrHash}/info");
        }

        public static KiraWebRequest.Builder<FileInfo> GetFileById(this IKiraWebRequest web, int id)
        {
            return web.GetFileByIdOrHash(id.ToString());
        }

        public static KiraWebRequest.Builder<bool> CheckFileExistByIdOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New<bool>().Get($"file/{idOrHash}/exists");
        }

        public static KiraWebRequest.Builder<bool> CheckFileExistById(this IKiraWebRequest web, int id)
        {
            return web.CheckFileExistByIdOrHash(id.ToString());
        }
    }
}