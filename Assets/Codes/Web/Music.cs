using Newtonsoft.Json;
using System.Collections.Generic;
using Web.Auth;

namespace Web.Music
{
    public class MusicLite
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;
    }

    public class MusicInfo : JsonWithTimeStamps
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public UserLite Uploader;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("background")]
        public string Background;

        [JsonProperty("length")]
        public float Length;

        [JsonProperty("bpm")]
        public List<float> Bpm = new List<float>();

        [JsonProperty("preview")]
        public List<float> Preview = new List<float>();

        [JsonProperty("source")]
        public string Source = "";

        [JsonProperty("language")]
        public string Language = "";

        [JsonProperty("style")]
        public string Style = "";
    }

    public class SongListResponse
    {
        [JsonProperty("music")]
        public List<MusicInfo> Songs = new List<MusicInfo>();
    }

    public class EditSongRequest
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("length")]
        public float Length;

        [JsonProperty("bpm")]
        public List<float> Bpm = new List<float>();

        [JsonProperty("preview")]
        public List<float> Preview = new List<float>();

        [JsonProperty("source")]
        public string Source;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("style")]
        public string Style;
    }

    public class CreateSongRequest : EditSongRequest
    {
        [JsonProperty("hash")]
        public string Hash;
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder<SongListResponse> GetAllSongs(this IKiraWebRequest web, int offset = 0, int limit = 20)
        {
            return web.New<SongListResponse>().Get($"music/all?offset={offset}&limit={limit}");
        }

        public static KiraWebRequest.Builder<MusicInfo> GetSongByIdOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New<MusicInfo>().Get($"music/{idOrHash}/info");
        }

        public static KiraWebRequest.Builder<MusicInfo> GetSongById(this IKiraWebRequest web, int id)
        {
            return web.GetSongByIdOrHash(id.ToString());
        }

        public static KiraWebRequest.Builder<bool> CheckSongExistsByidOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New<bool>().Get($"music/{idOrHash}/exists");
        }

        public static KiraWebRequest.Builder<bool> CheckSongExistsById(this IKiraWebRequest web, int id)
        {
            return web.CheckSongExistsByidOrHash(id.ToString());
        }

        public static KiraWebRequest.Builder<int> CreateSong(this IKiraWebRequest web, CreateSongRequest req)
        {
            return web.New<int>().UseTokens().SetReq(req).Post($"music/create");
        }

        public static KiraWebRequest.Builder<object> EditSong(this IKiraWebRequest web, string idOrHash, EditSongRequest req)
        {
            return web.New().UseTokens().SetReq(req).Post($"music/{idOrHash}/edit");
        }

        public static KiraWebRequest.Builder<object> EditSong(this IKiraWebRequest web, int id, EditSongRequest req)
        {
            return web.EditSong(id.ToString(), req);
        }

        public static KiraWebRequest.Builder<SongListResponse> SearchForSong(this IKiraWebRequest web, string keyword, int offset = 0, int limit = 20)
        {
            return web.New<SongListResponse>().Get($"music/search?offset={offset}&limit={limit}&keyword={keyword}");
        }
    }
}