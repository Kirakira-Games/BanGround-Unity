using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Zenject;
using BanGround.Audio;
using TagLib;
using ModestTree;
using BanGround.Utils;
using System.Runtime.InteropServices;

public class ChartCreator : MonoBehaviour
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private ILoadingBlocker loadingBlocker;

    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public const int ChartVersion = 1;
    public Button Blocker;
    public Toggle[] Toggles;

    private cHeader cHeader => chartListManager.current.header;
    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;

    public void Show()
    {
        Blocker.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Toggles[(int)chartListManager.current.difficulty].isOn = true;
    }

    private int SelectedDifficulty()
    {
        for (int i = 0; i < Toggles.Length; i++)
        {
            if (Toggles[i].isOn)
                return i;
        }
        return -1;
    }

    private mHeader CreateMHeader(string title, string artist, float len)
    {
        var header = new mHeader
        {
            mid = dataLoader.GenerateMid(),
            title = title,
            artist = artist,
            length = len,
            BPM = new float[] { 0.0f },
            preview = new float[] { 0.0f, 0.0f }
        };

        return header;
    }
    private cHeader CreateHeader(int mid = -1, string cover = "")
    {
        bool copyInfo = mid == -1;

        return new cHeader
        {
            version = ChartVersion,

            sid = dataLoader.GenerateSid(),
            mid = copyInfo ? cHeader.mid : mid,

            author = UserInfo.isOffline ? "Guest" : UserInfo.user.Username,
            authorNick = UserInfo.isOffline ? "Guest" : UserInfo.user.Nickname,

            backgroundFile = new BackgroundFile
            {
                pic = cover
            },

            preview = (!copyInfo || cHeader.preview == null) ? new float[] { 0.0f, 0.0f } : cHeader.preview.ToArray(),
            tag = (!copyInfo || cHeader.tag == null) ? new List<string>() : cHeader.tag.ToList()
        };
    }

    private V2.Chart CreateChart(Difficulty difficulty, int level)
    {
        var chart = new V2.Chart();
        chart.difficulty = difficulty;
        chart.level = level;
        var group = V2.TimingGroup.Default();
        chart.groups.Add(group);
        chart.bpm.Add(new V2.ValuePoint
        {
            beat = new int[] { 0, 0, 1 },
            value = 120
        });
        return chart;
    }

    public void Hide()
    {
        Blocker.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Duplicate()
    {
        dataLoader.DuplicateKiraPack(cHeader);
        SceneManager.LoadScene("Select");
    }

    public void NewChartSet()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }
        // Create header
        var header = CreateHeader();
        dataLoader.SaveHeader(header);

        // Create chart
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var chart = CreateChart((Difficulty)difficulty, level);
        dataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

        // Reload scene
        cl_lastdiff.Set(difficulty);
        cl_lastsid.Set(header.sid);
        SceneManager.LoadScene("Select");
    }

    public void NewDifficulty()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }
        if (cHeader.difficultyLevel[difficulty] != -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "This difficulty already exists.");
            return;
        }
        // Create chart
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var chart = CreateChart((Difficulty)difficulty, level);
        dataLoader.SaveChart(chart, cHeader.sid, (Difficulty)difficulty);

        // Reload scene
        cl_lastdiff.Set(difficulty);
        chartListManager.current.difficulty = (Difficulty)difficulty;
        SceneManager.LoadScene("Select");
    }

    public static bool RequestAirdrop = false;
    public static byte[] AirdroppedFile = null;

    public async void ImportMusic()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();


        var task = WaitForAirdrop(tokenSource.Token);

        loadingBlocker.Show("Waiting for airdrop (You must drop a ogg music!!!)...", tokenSource.Cancel);

        await task;

        loadingBlocker.Close();
    }

    public async UniTask WaitForAirdrop(CancellationToken token = default)
    {
        try
        {
            int difficulty = SelectedDifficulty();

            byte[] file = null;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var sfd = await new SelectFileDialog()
                .SetFilter("OGG Audio\0*.ogg\0")
                .SetTitle("Select Audio file")
                .SetDefaultExt("ogg")
                .ShowAsync();

            if(sfd.IsSucessful)
            {
                file = System.IO.File.ReadAllBytes(sfd.File);
            }
            else
            {
                throw new System.OperationCanceledException();
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            bool cancel = false;
            string audioPath = null;

            NativeGallery.GetAudioFromGallery(path =>
            {
                if (path == null)
                    cancel = true;

                audioPath = path;
            });

            await UniTask.WaitUntil(() => cancel || audioPath != null).WithCancellation(token);

            if (cancel)
            {
                throw new System.OperationCanceledException();
            }

            file = System.IO.File.ReadAllBytes(audioPath);

#else
            RequestAirdrop = true;

            await UniTask.Delay(1500).WithCancellation(token);

            Application.OpenURL("http://127.0.0.1:8088/");

            await UniTask.WaitUntil(() => AirdroppedFile != null).WithCancellation(token);

            RequestAirdrop = false;

            file = AirdroppedFile;
#endif

            var title = "New Song";
            var artist = "Unknown Artist";
            var len = -1.0f;

            TagLib.File tagFile = null;

            try
            {
                tagFile = TagLib.File.Create(new AudioFileAbstraction(file));
            }
            catch (UnsupportedFormatException)
            {
                messageBannerController.ShowMsg(LogLevel.ERROR, "Unsupported file format!");
                throw new InvalidDataException("Unsupported file format!");
            }

            title = tagFile.Tag.Title ?? "New Song";
            artist = tagFile.Tag.Performers == null ? "Unknown Artist" : tagFile.Tag.Performers.Join(", ");
            len = (float)tagFile.Properties.Duration.TotalSeconds;

            byte[] cover = null;
            string coverExt = null;

            if (tagFile.Tag.Pictures.Length > 0)
            {
                var pic = tagFile.Tag.Pictures[0];

                cover = pic.Data.ToArray();
                coverExt = pic.MimeType.Replace("image/", ".");
            }


            if (cover == null)
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                await sfd.SetFilter("File contains cover\0*.jpg;*.png;*.flac;*.mp3;*.aac\0")
                .SetTitle("Select Cover file")
                .SetDefaultExt("jpg")
                .ShowAsync();

                if (sfd.IsSucessful)
                {
                    var coverfi = new FileInfo(sfd.File);

                    if(coverfi.Extension == ".jpg" || coverfi.Extension == ".png")
                    {
                        cover = System.IO.File.ReadAllBytes(coverfi.FullName);
                    }
                    else
                    {
                        var coverFile = TagLib.File.Create(coverfi.FullName);

                        if (coverFile.Tag.Pictures.Length > 0)
                        {
                            var pic = coverFile.Tag.Pictures[0];

                            cover = pic.Data.ToArray();
                            coverExt = pic.MimeType.Replace("image/", ".");
                        }
                    }
                }
#elif (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                cancel = false;
                string coverPath = null;

                NativeGallery.GetImageFromGallery(path =>
                {
                    if (path == null)
                        cancel = true;

                    coverPath = path;
                });

                await UniTask.WaitUntil(() => cancel || coverPath != null).WithCancellation(token);

                if (!cancel && coverPath != null)
                {
                    var texture = NativeGallery.LoadImageAtPath(coverPath, -1, false, false, true);
                    cover = texture.EncodeToJPG(75);

                    coverExt = ".jpg";

                    Destroy(texture);
                }
#endif
            }

            // Create mheader
            var mheader = CreateMHeader(title, artist, len);
            dataLoader.SaveHeader(mheader, file);

            // Create header
            var header = CreateHeader(mheader.mid, cover == null ? default : "bg" + coverExt);
            dataLoader.SaveHeader(header, coverExt, cover);

            // Create chart
            int clamped = Mathf.Clamp(difficulty, 0, 3);
            int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
            var chart = CreateChart((Difficulty)difficulty, level);
            dataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

            // Reload scene
            cl_lastdiff.Set(difficulty);
            chartListManager.current.difficulty = (Difficulty)difficulty;
            cl_lastsid.Set(header.sid);
            SceneManager.LoadScene("Select");

#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
            AirdroppedFile = null;
#endif
        }
        catch (System.OperationCanceledException)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Canceled");

            RequestAirdrop = false;
            AirdroppedFile = null;
        }

    }

    public void 还没做好()
    {
        messageBannerController.ShowMsg(LogLevel.INFO, "Coming soon!");
    }
}
