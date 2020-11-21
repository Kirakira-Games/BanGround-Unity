using System.Collections.Generic;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Linq;

using System;
using BanGround;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BanGround.Web;
using BanGround.Web.Chart;
using BanGround.Web.File;
using BanGround.Web.Music;
using BanGround.Web.Upload;
using BanGround.Identity;

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
        [Inject]
        private IAccountManager accountManager;

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
            if (accountManager.isOfflineMode)
            {
                messageBanner.ShowMsg(LogLevel.ERROR, "You're currently offline.");
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
                Debug.Log("Music hash = " + file.Info.Hash);
                return (await web.GetSongByIdOrHash(file.Info.Hash).Fetch()).Id;
            }
            catch (KiraWebException)
            {
                return -1;
            }
        }

        private async UniTask<int> FindExistingChart(int id)
        {
            try
            {
                return (await web.GetChartById(id).Fetch()).Id;
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
            chartHeader.Sanitize(musicHeader);
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
            // Check if music already exists
            loadingBlocker.SetText("Discussing with the server about this song...");
            int mid = await FindExistingSong(musicFiles[0]);
            loadingBlocker.SetText("Updating local data...");
            if (mid >= 0)
            {
                mid = IDRouterUtil.ToFileId(ChartSource.BanGround, mid);
                dataLoader.MoveMusic(musicHeader.mid, mid);
                Refresh();
            }
            else if (musicSource != ChartSource.Local)
            {
                mid = dataLoader.GenerateMid();
                dataLoader.MoveMusic(musicHeader.mid, mid);
                Refresh();
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
            if (chartSource == ChartSource.BanGround)
            {
                loadingBlocker.SetText("Discussing with the server about this chart...");
                int sid = await FindExistingChart(chartId);
                loadingBlocker.SetText("Updating local data...");
                if (sid < 0)
                {
                    sid = dataLoader.GenerateSid();
                }
                else
                {
                    sid = IDRouterUtil.ToFileId(ChartSource.BanGround, sid);
                }
                dataLoader.MoveChart(chartHeader.sid, sid);
                Refresh();
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
            await BatchUpload(musicFiles);
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
            loadingBlocker.SetText("Uploading files", true);
            var uploads = await BatchUpload(chartFiles);

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
            loadingBlocker.SetText("Updating chart resources...");
            await web.UpdateChart(chartId, new UpdateChartRequest
            {
                Difficulty = chartHeader.difficultyLevel,
                Resources = chartFiles.Select(x => new FilenameHash {
                    Name = x.Filename,
                    Hash = x.Info.Hash
                }).ToList()
            }).Send();
            // Update chart version
            loadingBlocker.SetText("Updating chart version...");
            chartHeader.version = (await web.GetChartById(chartId).Fetch()).Version;
            dataLoader.SaveHeader(chartHeader);
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
            await UniTask.DelayFrame(1);

            // Prepare chart
            loadingBlocker.SetText("Preparing chart files");
            if (!await PrepareChart())
                return;
            await UniTask.DelayFrame(1);

            // Calc cost
            loadingBlocker.SetText("Calculating cost...");
            allFiles = musicFiles == null ? chartFiles : musicFiles.Concat(chartFiles).ToList();
            if (!await CalcFish(allFiles))
                return;
            await UniTask.DelayFrame(1);

            // Upload song
            if (!await CreateMusic())
                return;
            await UniTask.DelayFrame(1);

            // Create chart
            if (!await CreateChart())
                return;
            await UniTask.DelayFrame(1);

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
                await UniTask.DelayFrame(1);
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

        private async UniTask<List<UploadResponse>> BatchUpload(List<FileInfo> files)
        {
            var uploads = files.GroupBy(file => file.IsDuplicate).ToDictionary(g => g.Key, g => g.ToList());
            var ret = new List<UploadResponse>();
            if (uploads.ContainsKey(false))
            {
                int count = uploads[false].Count;
                int current = 0;
                foreach (var file in uploads[false])
                {
                    loadingBlocker.SetProgress(current, count);
                    ret.Add(await web.UploadFile(file.Filename, file.Content).Fetch());
                    current++;
                }
            }
            if (uploads.ContainsKey(true))
            {
                await web.ClaimFiles(uploads[true].Select(file => file.Info.Hash).ToList()).Send();
            }
            return ret;
        }
    }
}