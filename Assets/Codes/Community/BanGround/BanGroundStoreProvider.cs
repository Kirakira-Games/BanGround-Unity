using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;
using System.Linq;
using BanGround.Web.Chart;
using BanGround.Web.Auth;
using BanGround.Web;
using BanGround.Web.Music;
using System;

namespace BanGround.Community
{
    public class BanGroundStoreProvider : IStoreProvider
    {
        [Inject]
        private IKiraWebRequest web;
        [Inject]
        private IDownloadManager downloadManager;
        [Inject]
        private IFileSystem fs;
        [Inject]
        private IDataLoader dataLoader;

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

        public async UniTask<IDownloadTask> AddToDownloadList(SongItem song, ChartItem chart)
        {
            Debug.Assert(chart.Source == ChartSource.BanGround);
            var task = new DownloadTaskGroup(chart.Source.ToString() + chart.Id);

            // Download music
            task.AddTask(new BanGroundHeaderDownloadTask(web, dataLoader, song.Id, false));
            // Download music resources
            var songResources = await web.GetMusicResources(song.Id).Fetch();
            int mid = IDRouterUtil.ToFileId(ChartSource.BanGround, song.Id);
            Debug.Assert(songResources.Count == 1);
            foreach (var file in songResources)
            {
                task.AddTask(new WebClientDownloadTask(file.File.Url, dataLoader.GetMusicPath(mid), fs));
            }

            // Download chart
            var resources = await web.GetChartResources(chart.Id).Fetch();
            // Download chart header
            task.AddTask(new BanGroundHeaderDownloadTask(web, dataLoader, chart.Id, true)
            {
                resources = resources
            });
            // Download chart resources
            int sid = IDRouterUtil.ToFileId(ChartSource.BanGround, chart.Id);
            foreach (var file in resources)
            {
                task.AddTask(new WebClientDownloadTask(file.File.Url, dataLoader.GetChartResource(sid, file.Name), fs));
            }
            
            // Finalize
            downloadManager.AddTask(task);
            return task;
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