using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Web.Auth;
using Web.Music;

namespace Web.Chart
{
    public class ChartInfo : JsonWithTimeStamps
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public UserLite Uploader;

        [JsonProperty("music")]
        public MusicLite Music;

        [JsonProperty("background")]
        public string Background;

        [JsonProperty("difficulty")]
        public List<int> Difficulty = new List<int>();

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("isfav")]
        public bool IsFav;

        [JsonProperty("favorite")]
        public bool Favorite;

        [JsonProperty("hot")]
        public bool Hot;

        [JsonProperty("statusAt")]
        [JsonConverter(typeof(ISODateToStringConverter))]
        public DateTime StatusAt;
    }

    public class ChartListResponse
    {
        [JsonProperty("chart")]
        public List<ChartInfo> Charts = new List<ChartInfo>();
    }

    public class CreateChartRequest
    {
        [JsonProperty("music")]
        public int MusicId;

        [JsonProperty("background")]
        public string Background;

        [JsonProperty("description")]
        public string Description;
    }

    public class FilenameHash
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("hash")]
        public string Hash;
    }

    public class UpdateChartRequest
    {
        [JsonProperty("level")]
        public int Level;

        [JsonProperty("resources")]
        public List<FilenameHash> Resources = new List<FilenameHash>();
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

    public class CreateSongRequest : EditSongRequest
    {
        [JsonProperty("hash")]
        public string Hash;
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder<ChartListResponse> GetAllCharts(this IKiraWebRequest web, int offset = 0, int limit = 20)
        {
            return web.New<ChartListResponse>().UseTokens().Get($"chart/all?offset={offset}&limit={limit}");
        }

        /// <summary>
        /// Update one chart of certain difficulty of a chart set.
        /// </summary>
        /// <returns>A list of URLs to the new resources.</returns>
        public static KiraWebRequest.Builder<List<string>> UpdateChart(this IKiraWebRequest web, UpdateChartRequest req, int id, Difficulty difficulty)
        {
            int diff = (int)difficulty;
            return web.New<List<string>>().UseTokens().SetReq(req).Post($"chart/{id}/{diff}/update");
        }

        public static KiraWebRequest.Builder<ChartInfo> GetChartById(this IKiraWebRequest web, int id)
        {
            return web.New<ChartInfo>().UseTokens().Get($"chart/{id}/info");
        }

        public static KiraWebRequest.Builder<int> CreatChartSet(this IKiraWebRequest web, CreateChartRequest req)
        {
            return web.New<int>().UseTokens().SetReq(req).Post("chart/create");
        }

        public static KiraWebRequest.Builder<object> EditChartSet(this IKiraWebRequest web, CreateChartRequest req, int id)
        {
            return web.New().UseTokens().SetReq(req).Post($"chart/{id}/edit");
        }

        public static KiraWebRequest.Builder<List<FileDownloadInfo>> GetChartResources(this IKiraWebRequest web, int id, Difficulty difficulty)
        {
            int diff = (int)difficulty;
            return web.New<List<FileDownloadInfo>>().UseTokens().Get($"chart/{id}/{diff}/resources");
        }
    }
}