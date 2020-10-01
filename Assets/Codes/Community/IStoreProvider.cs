using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Web;
using Newtonsoft.Json;

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
    }

    public class SongItem
    {
        public ChartSource Source;
        public int Id;
        public string Title;
        public string Artist;
        public string BackgroundUrl;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ChartItem
    {
        public ChartSource Source;
        public int Id;
        public UserItem Uploader;
        public List<int> Difficulty;
        public string BackgroundUrl;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public interface IStoreProvider
    {
        UniTask<List<SongItem>> Search(string keyword, int offset, int limit);
        UniTask<List<ChartItem>> GetCharts(int mid, int offset, int limit);
        UniTask<bool> AddToDownloadList(ChartItem item);
        void Cancel();
    }
}