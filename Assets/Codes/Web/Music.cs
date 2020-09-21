using Boo.Lang;
using Newtonsoft.Json;
using Web.Auth;

namespace Web.Music
{
    public class MusicInfo : JsonWithTimeStamps
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public User Uploader;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("background")]
        public string Background;

        [JsonProperty("length")]
        public float Length;

        [JsonProperty("bpm")]
        public List<int> Bpm = new List<int>();

        [JsonProperty("tags")]
        public List<string> Tags = new List<string>();

        [JsonProperty("source")]
        public string Source = "";

        [JsonProperty("language")]
        public string Language = "";

        [JsonProperty("style")]
        public string Style = "";
    }

    public class EditSongRequest
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("background")]
        public string Background;

        [JsonProperty("length")]
        public float Length;

        [JsonProperty("bpm")]
        public List<int> Bpm = new List<int>();

        [JsonProperty("tags")]
        public List<string> Tags = new List<string>();

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
        public static KiraWebRequest.Builder GetAllSongs(this IKiraWebRequest web, int offset = 0, int limit = 20)
        {
            return web.New().Get($"music/all?offset={offset}&limit={limit}");
        }

        public static KiraWebRequest.Builder GetSongsByUploader(this IKiraWebRequest web, int uid, int offset = 0, int limit = 20)
        {
            return web.New().Get($"user/{uid}/music?offset={offset}&limit={limit}");
        }

        public static KiraWebRequest.Builder GetSongByidOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New().Get($"music/{idOrHash}/info");
        }

        public static KiraWebRequest.Builder GetSongById(this IKiraWebRequest web, int id)
        {
            return web.GetSongByidOrHash(id.ToString());
        }

        public static KiraWebRequest.Builder CheckSongExistsByidOrHash(this IKiraWebRequest web, string idOrHash)
        {
            return web.New().Get($"music/{idOrHash}/exists");
        }

        public static KiraWebRequest.Builder CheckSongExistsById(this IKiraWebRequest web, int id)
        {
            return web.CheckSongExistsByidOrHash(id.ToString());
        }

        public static KiraWebRequest.Builder CreateSong(this IKiraWebRequest web, CreateSongRequest req)
        {
            return web.New().UseTokens().SetReq(req).Post($"music/create");
        }

        public static KiraWebRequest.Builder EditSong(this IKiraWebRequest web, string idOrHash, EditSongRequest req)
        {
            return web.New().UseTokens().SetReq(req).Post($"music/{idOrHash}/create");
        }

        public static KiraWebRequest.Builder EditSong(this IKiraWebRequest web, int id, EditSongRequest req)
        {
            return web.EditSong(id.ToString(), req);
        }

        public static KiraWebRequest.Builder SearchForSong(this IKiraWebRequest web, string keyword, int offset = 0, int limit = 20)
        {
            return web.New().Get($"music/search?offset={offset}&limit={limit}&keyword={keyword}");
        }
    }
}