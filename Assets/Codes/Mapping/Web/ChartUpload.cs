using System.Collections.Generic;
using Zenject;
using Web;
using UniRx.Async;
using Web.Upload;
using System.IO;
using System.Linq;

using FileResponse = Web.Upload.File;
using System;

namespace BGEditor
{
    class FileInfo
    {
        public FileHashSize Info;
        public string Filename;
        public byte[] Content;

        public FileInfo(string name, byte[] content)
        {
            Filename = name;
            Content = content;
            Info.Size = content.Length;
            Info.Hash = Util.Hash(content);
        }
    }

    public class ChartUpload
    {
        [Inject]
        IDataLoader dataLoader;
        [Inject]
        IKiraWebRequest web;
        [Inject]
        IMessageBox messageBox;
        [Inject]
        IMessageBannerController messageBanner;
        [Inject]
        ILoadingBlocker loadingBlocker;
        [Inject]
        IChartCore chartCore;
        [Inject]
        IChartListManager chartList;

        public async UniTaskVoid UploadChart(int sid)
        {
            if (UserInfo.isOffline)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "You're current offline.");
                return;
            }

            if (!await messageBox.ShowMessage("Upload Chart", "You need to save the chart before uploading. Continue?"))
            {
                return;
            }

            chartCore.Save();

            loadingBlocker.Show("Uploading Chart...");

            try
            {
                await StartUploadChart();
            }
            catch (KiraWebException e)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, $"[{e.RetCode}] {e.Message}");
            }
            catch (Exception e)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, e.Message);
            }

            loadingBlocker.Close();
        }

        private async UniTaskVoid StartUploadChart()
        {
            var cHeader = chartList.current.header;

            // Check music
            var mSource = IDRouterUtil.GetSource(cHeader.mid, out int musicId);
            if (mSource != ChartSource.Local && mSource != ChartSource.BanGround)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "Unsupported source.\nIf you are not using the latest version of BanGround, please update.");
                return;
            }
            if (mSource == ChartSource.Local && !await messageBox.ShowMessage("Upload Chart", "This is a local song.\nWould you like to upload this song?"))
            {
                return;
            }

            // Check chart
            var cSource = IDRouterUtil.GetSource(cHeader.sid, out int id);
            if (cSource != ChartSource.Local && cSource != ChartSource.BanGround)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "Unsupported source.\nIf you are not using the latest version of BanGround, please update.");
                return;
            }
            string message = cSource == ChartSource.Local
                ? "This is a local chart.\nWould you like to create this chart?"
                : "This chart has already been uploaded.\nWould you like to modify this chart?\n(This may fail if you don't have access)";
                   
            if (!await messageBox.ShowMessage("Upload Chart", message))
            {
                return;
            }

            // Calc cost
            List<FileInfo> musicFiles = new List<FileInfo>();
            if (mSource == ChartSource.Local)
            {
                loadingBlocker.SetText("Preparing music files");
                musicFiles = await GenerateFileList(DataLoader.MusicDir + cHeader.sid);
            }

            loadingBlocker.SetText("Preparing chart files");
            var chartFiles = await GenerateFileList(dataLoader.GetChartResource(cHeader.sid, ""));

            var allFiles = musicFiles.Concat(chartFiles).ToList();

            loadingBlocker.SetText("Calculating cost...");
            var duplicates = await CalcFish(allFiles);
            if (duplicates == null)
            {
                return;
            }

            // Upload song
            int mid = cHeader.mid;
            if (mSource == ChartSource.Local)
            {
                // Upload song
                loadingBlocker.SetText("Uploading song");
                var responses = await BatchUpload(UploadType.Music, musicFiles);
                FileResponse resp = null;
                for (int i = 0; i < musicFiles.Count; i++)
                {
                    if (musicFiles[i].Filename == dataLoader.GetMusicPath(mid))
                    {
                        resp = responses[i];
                        break;
                    }
                }
                if (resp == null)
                {
                    // Should not happen
                    throw new InvalidDataException("Music file not found.");
                }

                // Create song
                loadingBlocker.SetText("Creating song...");
                var mHeader = dataLoader.GetMusicHeader(mid);
                mid = await CreateSong(mHeader, resp);
                if (mid == -1)
                {
                    return;
                }

                // TODO: update all mids, including references by other songs
                loadingBlocker.SetText("Updating local data (Music ID)...");
                await UniTask.DelayFrame(0);
            }

            // Upload chart
            int sid = cHeader.sid;
            if (cSource == ChartSource.Local)
            {
                loadingBlocker.SetText("Creating chart...");
                sid = await CreateChart(cHeader);
                if (sid == -1)
                {
                    return;
                }
                // TODO: update sid
                loadingBlocker.SetText("Updating local data (Chart ID)...");
                await UniTask.DelayFrame(0);
            }

            // Upload resources
            loadingBlocker.SetText("Uploading chart");
            var chartRes = await BatchUpload(UploadType.Chart, chartFiles);
            
            // TODO: Call API to assocaited these resources.
        }

        private async UniTask<List<FileInfo>> GenerateFileList(string prefix)
        {
            var files = KiraFilesystem.Instance.ListFiles((path) =>
            {
                path = path.Replace("\\", "/");
                return path.StartsWith(prefix);
            });
            var ret = new List<FileInfo>();
            int filesCount = files.Length;
            int currentCount = 0;
            foreach (var file in files)
            {
                var path = file.Split('/');
                var name = path[path.Length - 1];
                ret.Add(new FileInfo(name, KiraFilesystem.Instance.Read(file)));
                loadingBlocker.SetProgress(currentCount, filesCount);
                currentCount++;
                await UniTask.DelayFrame(0);
            }
            return ret;
        }

        private async UniTask<List<bool>> CalcFish(List<FileInfo> files)
        {
            var req = files.Select(file => file.Info).ToList();
            var fishDelta = await web.DoCalcUploadCost(req);
            if (fishDelta.Fish < 0)
            {
                messageBanner.ShowMsg(LogLevel.INFO, $"Need {fishDelta.Required} fish, but you only have {fishDelta.Fish + fishDelta.Required}");
                return null;
            }
            if (!await messageBox.ShowMessage("Fish Pay", $"Cost: {fishDelta.Required}. You have {fishDelta.Fish} fish after the payment.\nContinue?"))
            {
                return null;
            }
            return fishDelta.Duplicates;
        }

        private async UniTask<int> CreateSong(mHeader header, FileResponse file)
        {
            //TODO
            return -1;
        }

        private async UniTask<int> CreateChart(cHeader header)
        {
            //TODO
            return -1;
        }

        private async UniTask<List<FileResponse>> BatchUpload(UploadType type, List<FileInfo> files)
        {
            var ret = new List<FileResponse>();
            int count = files.Count;
            int current = 0;
            foreach (var file in files)
            {
                loadingBlocker.SetProgress(current, count);
                await web.DoUploadFile(type, file.Content);
                current++;
            }
            return ret;
        }
    }
}