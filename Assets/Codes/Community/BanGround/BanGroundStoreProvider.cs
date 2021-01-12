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
using BanGround.Web.File;

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
                return background.Replace("/storage/", web.ServerSite + "/storage/");
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
                BackgroundUrl = ToBackgroundUrl(info.Background),
                DiffRange = info.DiffRange.ToArray()
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

        private void ResourceSanityCheck(List<FileDownloadInfo> resources)
        {
            foreach (var file in resources)
            {
                if (file.File.Url.StartsWith("/"))
                {
                    file.File.Url = web.ServerSite + file.File.Url;
                }
            }
        }

        public async UniTask<IDownloadTask> AddToDownloadList(SongItem song, ChartItem chart)
        {
            Debug.Assert(chart.Source == ChartSource.BanGround);
            var task = new DownloadTaskGroup(chart.Source.ToString() + chart.Id);

            // Download music
            task.AddTask(new BanGroundHeaderDownloadTask(web, dataLoader, song.Id, false));
            // Download music resources
            var songResources = await web.GetMusicResources(song.Id).Fetch();
            ResourceSanityCheck(songResources);
            int mid = IDRouterUtil.ToFileId(ChartSource.BanGround, song.Id);
            Debug.Assert(songResources.Count == 1);
            foreach (var file in songResources)
            {
                task.AddTask(new WebClientDownloadTask(file.File.Url, dataLoader.GetMusicPath(mid), fs));
            }

            // Download chart
            int sid = IDRouterUtil.ToFileId(ChartSource.BanGround, chart.Id);
            var resources = await web.GetChartResources(chart.Id).Fetch();
            ResourceSanityCheck(resources);

            // Download background
            string backgroundUrl = string.IsNullOrEmpty(chart.BackgroundUrl) ? song.BackgroundUrl : chart.BackgroundUrl;
            string backgroundFileName = null;
            if (backgroundUrl != null) {
                backgroundFileName = "bg.jpg";
                var alreadyInList = resources.Find(file => file.File.Url == backgroundUrl);
                if (alreadyInList == null)
                {
                    task.AddTask(new WebClientDownloadTask(backgroundUrl, dataLoader.GetChartResource(sid, backgroundFileName), fs));
                }
                else
                {
                    backgroundFileName = alreadyInList.Name;
                }
            }

            // Download chart resources
            foreach (var file in resources)
            {
                string url = file.File.Url;
                if (url.StartsWith("/"))
                {
                    url = web.ServerAddr + url.Substring(1);
                }
                task.AddTask(new WebClientDownloadTask(url, dataLoader.GetChartResource(sid, file.Name), fs));
            }
            // Download chart header
            // Must be done at the end because the chart will be registered in the database
            task.AddTask(new BanGroundHeaderDownloadTask(web, dataLoader, chart.Id, true)
            {
                BackGround = backgroundFileName
            });
            
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