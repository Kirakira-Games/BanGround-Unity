using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Web.Music;
using Zenject;
using Web;
using System.Linq;
using Web.Chart;
using Web.Auth;

namespace BanGround.Community
{
    public class BanGroundStoreProvider : IStoreProvider
    {
        [Inject]
        private IKiraWebRequest web;

        private CancellationTokenSource mTokenSource = new CancellationTokenSource();

        private string ToBackgroundUrl(string background)
        {
            if (string.IsNullOrEmpty(background))
                return background;
            if (background.StartsWith("/storage/"))
                return background.Replace("/storage/", "https://beijing.aliyun.reikohaku.fun/storage/");
            return background;
        }

        private SongItem ToSongItem(MusicInfo info)
        {
            return new SongItem
            {
                Source = ChartSource.BanGround,
                Id = info.Id,
                Title = info.Title,
                Artist = info.Artist,
                BackgroundUrl = ToBackgroundUrl(info.Background)
            };
        }

        private UserItem ToUserItem(UserLite user)
        {
            return new UserItem
            {
                Username = user.Username,
                Nickname = user.Nickname
            };
        }

        private ChartItem ToChartItem(ChartInfo chart)
        {
            return new ChartItem
            {
                Source = ChartSource.BanGround,
                Id = chart.Id,
                Uploader = ToUserItem(chart.Uploader),
                BackgroundUrl = ToBackgroundUrl(chart.Background),
                Difficulty = chart.Difficulty
            };
        }

        public async UniTask<bool> AddToDownloadList(ChartItem item)
        {
            Debug.Log("Should add to download list: " + item);
            return false;
        }

        public void Cancel()
        {
            mTokenSource.Cancel();
            mTokenSource = new CancellationTokenSource();
        }

        public async UniTask<List<ChartItem>> GetCharts(int mid, int offset, int limit)
        {
            var charts = await web.GetChartsBySong(mid, offset, limit).Fetch().WithCancellation(mTokenSource.Token);
            return charts.Charts.Select(chart => ToChartItem(chart)).ToList();
        }

        public async UniTask<List<SongItem>> Search(string keyword, int offset, int limit)
        {
            SongListResponse songs;
            if (string.IsNullOrEmpty(keyword))
            {
                songs = await web.GetAllSongs(offset, limit).Fetch().WithCancellation(mTokenSource.Token);
            }
            else
            {
                songs = await web.SearchForSong(keyword, offset, limit).Fetch().WithCancellation(mTokenSource.Token);
            }
            return songs.Songs.Select(song => ToSongItem(song)).ToList();;
        }
    }
}