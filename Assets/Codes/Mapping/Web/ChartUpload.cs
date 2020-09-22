using System.Collections.Generic;
using Zenject;
using Web;
using UniRx.Async;
using Web.Upload;
using System.Linq;

using System;
using BanGround;
using Web.Music;
using UnityEngine;
using Web.Chart;
using Web.File;
using UnityEngine.UI;
using System.IO;

namespace BGEditor
{
    class FileInfo
    {
        public FileHashSize Info;
        public bool IsDuplicate;
        public string Filename;
        public byte[] Content;

        public FileInfo(string name, byte[] content)
        {
            Filename = name;
            Content = content;
            Info = new FileHashSize
            {
                Size = content.Length,
                Hash = Util.Hash(content)
            };
        }
    }

    public class ChartUpload : MonoBehaviour
    {
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IKiraWebRequest web;
        [Inject]
        private IMessageBox messageBox;
        [Inject]
        private IMessageBannerController messageBanner;
        [Inject]
        private ILoadingBlocker loadingBlocker;
        [Inject]
        private IChartCore chartCore;
        [Inject]
        private IChartListManager chartList;
        [Inject]
        private IFileSystem fs;

        // Chart related
        private cHeader chartHeader;
        private List<FileInfo> chartFiles;
        private ChartSource chartSource;
        private int chartId;

        // Music related
        private mHeader musicHeader;
        private List<FileInfo> musicFiles;
        private ChartSource musicSource;
        private int musicId;

        // All
        private List<FileInfo> allFiles;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => UploadChart().Forget());
        }

        public async UniTaskVoid UploadChart()
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
                Debug.LogError(e.StackTrace);
            }

            loadingBlocker.Close();
        }

        private async UniTask<int> FindExistingSong(FileInfo file)
        {
            try
            {
                Debug.Log("Hash = " + file.Info.Hash);
                return (await web.GetSongByIdOrHash(file.Info.Hash).Fetch()).Id;
            }
            catch (KiraWebException)
            {
                return -1;
            }
        }

        private void Refresh()
        {
            chartHeader = chartList.current.header;
            musicHeader = dataLoader.GetMusicHeader(chartHeader.mid);
            musicHeader.Sanitize();
            chartHeader.Sanitize(musicHeader.length);
            chartSource = IDRouterUtil.GetSource(chartHeader.sid, out chartId);
            musicSource = IDRouterUtil.GetSource(chartHeader.mid, out musicId);
        }

        private async UniTask<bool> PrepareMusic()
        {
            if (musicSource != ChartSource.Local && musicSource != ChartSource.BanGround)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "Unsupported source: " + musicSource.ToString());
                return false;
            }
            musicFiles = await GenerateFileList(DataLoader.MusicDir + chartHeader.mid);
            if (musicFiles.Count == 0)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "Corrupted music data.");
                return false;
            }
            // Find the main music file
            for (int i = 0; i < musicFiles.Count; i++)
            {
                var current = musicFiles[i];
                if (current.Filename.StartsWith(musicId + "."))
                {
                    musicFiles.RemoveAt(i);
                    musicFiles.Insert(0, current);
                    break;
                }
            }
            // Check source
            if (musicSource == ChartSource.Local)
            {
                // Check if music already exists
                loadingBlocker.SetText("Discussing with the server about this song...");
                int mid = await FindExistingSong(musicFiles[0]);
                if (mid >= 0)
                {
                    loadingBlocker.SetText("Updating local data...");
                    mid = IDRouterUtil.ToFileId(ChartSource.BanGround, mid);
                    dataLoader.MoveMusic(musicHeader.mid, mid);
                    Refresh();
                    return true;
                }
            }
            else
            {
                // Already exists, no need to upload.
                musicFiles = null;
            }
            return true;
        }

        private async UniTask<bool> PrepareChart()
        {
            if (chartSource != ChartSource.Local && chartSource != ChartSource.BanGround)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "Unsupported source.");
                return false;
            }
            chartFiles = await GenerateFileList(dataLoader.GetChartResource(chartHeader.sid, ""));
            return true;
        }

        private async UniTask<bool> CreateMusic()
        {
            if (musicSource != ChartSource.Local)
            {
                // Update song data
                loadingBlocker.SetText("Updating song...");
                try
                {
                    await web.EditSong(musicId, new EditSongRequest
                    {
                        Title = musicHeader.title,
                        Artist = musicHeader.artist,
                        Length = musicHeader.length,
                        Bpm = new List<float>(musicHeader.BPM),
                        Preview = musicHeader.preview.ToList()
                    }).Send();
                }
                catch (KiraWebException) { }
                return true;
            }
            // Upload song
            loadingBlocker.SetText("Uploading song");
            await BatchUpload(UploadType.Music, musicFiles);
            // Create song
            int mid = await web.CreateSong(new CreateSongRequest
            {
                Title = musicHeader.title,
                Artist = musicHeader.artist,
                Length = musicHeader.length,
                Bpm = new List<float>(musicHeader.BPM),
                Hash = musicFiles[0].Info.Hash,
                Preview = musicHeader.preview.ToList()
            }).Fetch();
            loadingBlocker.SetText("Updating local data...");
            mid = IDRouterUtil.ToFileId(ChartSource.BanGround, mid);
            dataLoader.MoveMusic(musicHeader.mid, mid);
            Refresh();
            return true;
        }

        private async UniTask<bool> CreateChart()
        {
            // Upload files
            loadingBlocker.SetText("Uploading files");
            var uploads = await BatchUpload(UploadType.Chart, chartFiles);

            // Find background file
            var bgFile = chartFiles.Find(file => file.Filename == chartHeader.backgroundFile.pic || file.Filename == chartHeader.backgroundFile.vid);
            if (bgFile == null)
            {
                // TODO: provide a default image
                messageBanner.ShowMsg(LogLevel.ERROR, "Background image is required.");
                return false;
            }
            var request = new CreateChartRequest
            {
                MusicId = musicId,
                Background = bgFile.Info.Hash,
                Description = "The author is too lazy to write anything.",
                Preview = chartHeader.preview.ToList(),
                Tags = chartHeader.tag
            };
            // Create chart data
            if (chartSource != ChartSource.Local)
            {
                // Update chart data
                loadingBlocker.SetText("Updating chart...");
                await web.EditChartSet(chartId, request).Send();
            }
            else
            {
                // Create chart set
                loadingBlocker.SetText("Creating chart...");
                int sid = await web.CreateChartSet(request).Fetch();
                loadingBlocker.SetText("Updating local data...");
                sid = IDRouterUtil.ToFileId(ChartSource.BanGround, sid);
                dataLoader.MoveChart(chartHeader.sid, sid);
                Refresh();
            }
            // Update resources
            loadingBlocker.SetText("Updating chart resources");
            for (int i = 0; i < chartHeader.difficultyLevel.Count; i++)
            {
                loadingBlocker.SetProgress(i, chartHeader.difficultyLevel.Count);
                int diff = chartHeader.difficultyLevel[i];
                if (diff == -1)
                    continue;
                await web.UpdateChart(chartId, (Difficulty)i, new UpdateChartRequest
                {
                    Level = diff,
                    Resources = chartFiles.Select(x => new FilenameHash {
                        Name = x.Filename,
                        Hash = x.Info.Hash
                    }).ToList()
                }).Send();
            }
            return true;
        }

        private async UniTask StartUploadChart()
        {
            // Initialize class members
            Refresh();
            chartFiles = null;
            allFiles = null;
            musicFiles = null;

            // Prepare music
            loadingBlocker.SetText("Preparing music files");
            if (!await PrepareMusic())
                return;
            await UniTask.DelayFrame(0);

            // Prepare chart
            loadingBlocker.SetText("Preparing chart files");
            if (!await PrepareChart())
                return;
            await UniTask.DelayFrame(0);

            // Calc cost
            loadingBlocker.SetText("Calculating cost...");
            allFiles = musicFiles == null ? chartFiles : musicFiles.Concat(chartFiles).ToList();
            if (!await CalcFish(allFiles))
                return;
            await UniTask.DelayFrame(0);

            // Upload song
            if (!await CreateMusic())
                return;
            await UniTask.DelayFrame(0);

            // Create chart
            if (!await CreateChart())
                return;
            await UniTask.DelayFrame(0);

            loadingBlocker.SetText("Wrapping it up...");
            await UniTask.Delay(5000);
        }

        private async UniTask<List<FileInfo>> GenerateFileList(string prefix)
        {
            var files = fs.Find((file) =>
            {
                return file.Name.StartsWith(prefix) && !file.Name.EndsWith("header.bin");
            });

            var ret = new List<FileInfo>();
            int filesCount = files.Count();
            int currentCount = 0;
            foreach (var file in files)
            {
                var name = Path.GetFileName(file.Name);
                ret.Add(new FileInfo(name, file.ReadToEnd()));
                loadingBlocker.SetProgress(currentCount, filesCount);
                currentCount++;
                await UniTask.DelayFrame(0);
            }
            return ret;
        }

        private async UniTask<bool> CalcFish(List<FileInfo> files)
        {
            var req = files.Select(file => file.Info).ToList();
            var fishDelta = await web.CalcUploadCost(req).Fetch();
            if (fishDelta.Fish < 0)
            {
                messageBanner.ShowMsg(LogLevel.INFO, $"Need {fishDelta.Required} fish, but you only have {fishDelta.Fish + fishDelta.Required}");
                return false;
            }
            if (!await messageBox.ShowMessage("Fish Pay", $"Cost: {fishDelta.Required}. You have {fishDelta.Fish} fish after the payment.\nContinue?"))
            {
                return false;
            }
            Debug.Assert(fishDelta.Duplicates.Count == files.Count);
            for (int i = 0; i < fishDelta.Duplicates.Count; i++)
            {
                files[i].IsDuplicate = fishDelta.Duplicates[i];
            }
            return true;
        }

        private async UniTask<List<UploadResponse>> BatchUpload(UploadType type, List<FileInfo> files)
        {
            var uploads = files.Where(file => !file.IsDuplicate).ToArray();
            var ret = new List<UploadResponse>();
            int count = uploads.Length;
            int current = 0;
            foreach (var file in uploads)
            {
                loadingBlocker.SetProgress(current, count);
                ret.Add(await web.UploadFile(type, file.Filename, file.Content).Fetch());
                current++;
            }
            return ret;
        }
    }
}