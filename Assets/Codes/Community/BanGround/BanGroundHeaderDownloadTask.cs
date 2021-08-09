using BanGround.Web;
using BanGround.Web.Chart;
using BanGround.Web.File;
using BanGround.Web.Music;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace BanGround.Community
{
    public class BanGroundHeaderDownloadTask : DownloadTaskBase, IDownloadTask
    {
        private IKiraWebRequest web;
        private IDataLoader dataLoader;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private UnityWebRequest req;
        public string BackGround;

        public int Id { get; private set; }
        /// <summary>
        /// Whether to download chart header or music header
        /// </summary>
        public bool IsChart { get; private set; }
        public override float Progress => req?.downloadProgress ?? 0;
        public override string Description { get; protected set; }
        public override DownloadState State { get; protected set; } = DownloadState.Preparing;

        public BanGroundHeaderDownloadTask(IKiraWebRequest web, IDataLoader dataLoader, int id, bool isChart)
        {
            this.web = web;
            this.dataLoader = dataLoader;
            Id = id;
            IsChart = isChart;
            string type = isChart ? "chart" : "music";
            Description = $"Download {type} header {id}";
        }

        ~BanGroundHeaderDownloadTask()
        {
            Cancel();
        }

        public override void Cancel()
        {
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
                cancellationToken = null;
            }
        }

        private async UniTask DownloadMusicHeader()
        {
            var builder = web.GetSongById(Id);
            var task = builder.Fetch().WithCancellation(cancellationToken.Token);
            req = builder.webRequest;
            var song = await task;
            var id = IDRouterUtil.ToFileId(ChartSource.BanGround, Id);
            var mHeader = new V2.mHeader
            {
                mid = id,
                title = song.Title,
                artist = song.Artist,
                preview = song.Preview.ToArray(),
                bpm = song.Bpm.ToArray(),
                length = song.Length
            };
            dataLoader.SaveHeader(mHeader);
        }

        private async UniTask DownloadChartHeader()
        {
            var builder = web.GetChartById(Id);
            var task = builder.Fetch().WithCancellation(cancellationToken.Token);
            req = builder.webRequest;
            var chart = await task;
            var id = IDRouterUtil.ToFileId(ChartSource.BanGround, Id);
            var cHeader = new V2.cHeader
            {
                version = chart.Version,
                sid = id,
                mid = IDRouterUtil.ToFileId(ChartSource.BanGround, chart.Music.Id),
                author = chart.Uploader.Username,
                authorNick = chart.Uploader.Nickname,
                backgroundFile = new V2.BackgroundFile
                {
                    pic = BackGround
                },
                preview = chart.Preview.ToArray(),
            };
            cHeader.tag.AddRange(chart.Tags);
            dataLoader.SaveHeader(cHeader);
        }

        public override async UniTask Start()
        {
            try
            {
                State = DownloadState.Downloading;
                if (IsChart)
                {
                    await DownloadChartHeader();
                }
                else
                {
                    await DownloadMusicHeader();
                }
                State = DownloadState.Finished;
                OnFinish.Invoke();
            }
            catch
            {
                State = DownloadState.Stopped;
                OnCancel.Invoke();
            }

        }
    }
}
