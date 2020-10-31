﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace BanGround.Community
{
    class WebClientDownloadTask : IDownloadTask
    {
        private IFileSystem fs;
        private string _dlAddr;
        private string _targetPath;

        private WebClient webClient;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public string Key { get; private set; }

        public float Progress { get; private set; } = 0.0f;

        public string Description { get; private set; } = "Preparing";

        public DownloadState State { get; private set; } = DownloadState.Preparing;

        public UnityEvent OnCancel { get; private set; } = new UnityEvent();

        public UnityEvent OnFinish { get; private set; } = new UnityEvent();

        /// <summary>
        /// Create a background download task
        /// </summary>
        /// <param name="address">Download address (url)</param>
        /// <param name="target">File path after downloaded (in fs)</param>
        public WebClientDownloadTask(string address, string target, IFileSystem fs)
        {
            this.fs = fs;

            _dlAddr = address;
            _targetPath = target;

            Key = address;
        }


        public void Cancel()
        {
            if (webClient == null)
                throw new Exception("Download not started!");

            _cancellationToken.Cancel();
        }

        public async UniTask Start()
        {
            webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            State = DownloadState.Downloading;
            Description = "Downloading";

            var bytes = await webClient.DownloadDataTaskAsync(new Uri(_dlAddr)).AsUniTask().WithCancellation(_cancellationToken.Token);

            if (!_cancellationToken.Token.IsCancellationRequested)
            {
                var file = fs.GetOrNewFile(_targetPath);
                using (var stream = file.Open(FileAccess.Write))
                {
                    stream.SetLength(bytes.Length);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }

                State = DownloadState.Finished;
                Description = "Finished";
                OnFinish.Invoke();
            }
            else
            {
                State = DownloadState.Stopped;
                Description = "Stopped";
                OnCancel.Invoke();
            }

            webClient.Dispose();
            webClient = null;
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage / 100.0f;
        }
    }
}