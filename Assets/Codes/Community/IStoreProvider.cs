using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using BanGround.Web;

namespace BanGround.Community
{
    public class UserItem
    {
        public string Username;
        public string Nickname;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static UserItem Anonymous => new UserItem
        {
            Username = "unknown",
            Nickname = "Anonymous"
        };
    }

    public class SongItem
    {
        public ChartSource Source;
        public int Id;
        public string Title;
        public string Artist;
        public string BackgroundUrl;
        public int[] DiffRange;

        public string ToDisplayString()
        {
            return $"[{Id}] {Title} - {Artist}";
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ChartItem
    {
        public ChartSource Source;
        /// <summary>
        /// Original ID in source. Will be converted by <see cref="IDRouterUtil"/>.
        /// </summary>
        public int Id;
        public UserItem Uploader;
        public List<int> Difficulty;
        public string BackgroundUrl;

        public string ToDisplayString()
        {
            return $"[{Id}] Difficulties: {string.Join(",", Difficulty)} - {Uploader.Nickname} ({Uploader.Username})";
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public interface IStoreProvider
    {
        UniTask<List<SongItem>> Search(string keyword, int offset, int limit);
        UniTask<List<ChartItem>> GetCharts(int mid, int offset, int limit);
        UniTask<IDownloadTask> AddToDownloadList(SongItem song, ChartItem chart);
        void Cancel();
    }
}
