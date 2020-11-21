#if false
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Networking;
using UnityEngine;

namespace BanGround.Community
{
    class BackgroundDownloadTask : DownloadTaskBase, IDownloadTask
    {
        const string DOWNLOAD_CACHE_FOLDER = "download_cache/";

        private IFileSystem fs;
        private BackgroundDownload _bgDL = null;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private string _dlAddr = null;
        private string _dlPath = null;
        private string _targetPath = null;

        public override float Progress
        { 
            get
            {
                if (_bgDL == null)
                    return 0.0f;

                return _bgDL.progress;
            }
        }

        public override string Description
        {
            get
            {
                if (_bgDL == null)
                    return "Download not started";

                if (_bgDL.status == BackgroundDownloadStatus.Downloading)
                    return $"Downloading {_dlAddr}, progress {Math.Floor(Progress * 100)}%";

                if (_bgDL.status == BackgroundDownloadStatus.Done)
                    return "Download sucessfull!";

                if (_bgDL.status == BackgroundDownloadStatus.Failed)
                    return _bgDL.error;

                return "NO DESCRIPTION";
            }
        }

        public override DownloadState State
        {
            get
            {
                if (_bgDL == null)
                    return DownloadState.Preparing;

                switch (_bgDL.status)
                {
                    case BackgroundDownloadStatus.Downloading:
                        return DownloadState.Downloading;
                    case BackgroundDownloadStatus.Done:
                        return DownloadState.Finished;
                    default:
                        return DownloadState.Stopped;
                }
            }
        }

        /// <summary>
        /// Create a background download task
        /// </summary>
        /// <param name="address">Download address (url)</param>
        /// <param name="target">File path after downloaded (in fs)</param>
        public BackgroundDownloadTask(string address, string target, IFileSystem fs)
        {
            this.fs = fs;

            _dlAddr = address;
            _dlPath = Guid.NewGuid().ToString("N");
            _targetPath = target;

            Key = address;
        }

        public override void Cancel()
        {
            if (_bgDL == null)
                throw new Exception("Download not started!");

            _cancellationToken.Cancel();
        }

        public override async UniTask Start()
        {
            if (_bgDL != null)
                throw new Exception("Download already started!");

            _bgDL = BackgroundDownload.Start(new Uri(_dlAddr), Path.Combine(DOWNLOAD_CACHE_FOLDER, _dlPath));

            await _bgDL.WithCancellation(_cancellationToken.Token);

            if(!_cancellationToken.Token.IsCancellationRequested)
            {
                if(_bgDL.status == BackgroundDownloadStatus.Failed)
                {
                    throw new Exception($"Download failed: {_bgDL.error}");
                }

                var file = fs.GetOrNewFile(_targetPath);
                var cacheFileInfo = new FileInfo(Path.Combine(Application.persistentDataPath, DOWNLOAD_CACHE_FOLDER, _dlPath));
                using (var stream = file.Open(FileAccess.Write))
                {
                    using (var filestream = cacheFileInfo.OpenRead())
                    {
                        stream.SetLength(filestream.Length);
                        await filestream.CopyToAsync(stream);
                    }
                }

                cacheFileInfo.Delete();
                OnFinish.Invoke();
            }
            else
            {
                OnCancel.Invoke();
            }
        }
    }
}
#endif
