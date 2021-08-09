using UnityEngine;
using UnityEngine.UI;
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
using AudioProvider;
using BanGround.Identity;
using V2;

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
    [Inject]
    private IAudioProvider audioProvider;
    [Inject]
    private IAccountManager accountManager;

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
            bpm = new float[] { 0.0f },
            preview = new float[] { 0.0f, 0.0f }
        };

        return header;
    }
    private cHeader CreateHeader(int mid = -1, string cover = "")
    {
        bool copyInfo = mid == -1;

        var ret = new cHeader
        {
            version = ChartVersion,

            sid = dataLoader.GenerateSid(),
            mid = copyInfo ? cHeader.mid : mid,

            author = accountManager.ActiveUser.Username,
            authorNick = accountManager.ActiveUser.Nickname,

            backgroundFile = new BackgroundFile
            {
                pic = cover
            },

            preview = (!copyInfo || cHeader.preview == null) ? new float[] { 0.0f, 0.0f } : cHeader.preview.ToArray(),

            difficultyLevel = new List<int> { -1, -1, -1, -1, -1 },
        };
        if (copyInfo && cHeader.tag != null) {
            ret.tag.AddRange(cHeader.tag);
        }
        return ret;
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
        SceneLoader.LoadScene("Select");
    }

    public void NewChartSet()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "CreateChart.DiffNotSelected".L());
            return;
        }
        // Create header
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var header = CreateHeader();
        header.difficultyLevel[difficulty] = level;
        dataLoader.SaveHeader(header);

        // Create chart
        var chart = CreateChart((Difficulty)difficulty, level);
        dataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

        // Reload scene
        cl_lastdiff.Set(difficulty);
        cl_lastsid.Set(header.sid);
        SceneLoader.LoadScene("Select");
    }

    public void NewDifficulty()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "CreateChart.DiffNotSelected".L());
            return;
        }
        if (cHeader.difficultyLevel[difficulty] != -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "CreateChart.DiffExists".L());
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
        SceneLoader.LoadScene("Select");
    }

    public static bool RequestAirdrop = false;
    public static byte[] AirdroppedFile = null;

    public async void ImportMusic()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "CreateChart.DiffNotSelected".L());
            return;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();


        var task = WaitForAirdrop(tokenSource.Token);

        loadingBlocker.Show("CreateChart.WaitForAudio".L(), tokenSource.Cancel);

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
                .SetFilter("Audio File\0*.ogg;*.mp3;*.aac\0")
                .SetTitle("CreateChart.SelectAudioFile".L())
                .SetDefaultExt("ogg")
                .ShowAsync();

            if (sfd.IsSucessful)
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

            string title = null;
            string artist = null;
            float len = -1.0f;

            TagLib.File tagFile = null;

            try
            {
                tagFile = TagLib.File.Create(new AudioFileAbstraction(file));
            }
            catch (UnsupportedFormatException)
            {
                messageBannerController.ShowMsg(LogLevel.ERROR, "CreateChart.UnsupportedAudioFormat".L());
                return;
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


            if (cover == null && false) // Maybe remove this feature?
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                await sfd.SetFilter("File contains cover\0*.jpg;*.png;*.flac;*.mp3;*.aac\0")
                .SetTitle("Select Cover file")
                .SetDefaultExt("jpg")
                .ShowAsync();

                if (sfd.IsSucessful)
                {
                    var coverfi = new FileInfo(sfd.File);

                    if (coverfi.Extension == ".jpg" || coverfi.Extension == ".png")
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
#if UNITY_IOS
                bool 
#endif
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

            byte[] transcoded = null;

            using (ITranscoder transcoder = new BassTranscoder(audioProvider) { Source = file })
            {
                loadingBlocker.SetText("CreateChart.ConvertingAudio".L(), true);
                loadingBlocker.SetProgress(transcoder);
                transcoded = await transcoder.DoAsync();
            }

            // Create mheader
            var mheader = CreateMHeader(title, artist, len);
            dataLoader.SaveHeader(mheader, transcoded);

            // Create header
            var header = CreateHeader(mheader.mid, cover == null ? default : "bg" + coverExt);

            // Create chart
            int clamped = Mathf.Clamp(difficulty, 0, 3);
            int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
            var chart = CreateChart((Difficulty)difficulty, level);
            dataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

            dataLoader.SaveHeader(header, coverExt, cover);

            // Reload scene
            cl_lastdiff.Set(difficulty);
            chartListManager.current.difficulty = (Difficulty)difficulty;
            cl_lastsid.Set(header.sid);
            SceneLoader.LoadScene("Select");

#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
            AirdroppedFile = null;
#endif
        }
        catch (System.OperationCanceledException)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Canceled".L());

            RequestAirdrop = false;
            AirdroppedFile = null;
        }

    }

    public void 还没做好()
    {
        messageBannerController.ShowMsg(LogLevel.INFO, "Findstr.NotDoneYet".L());
    }
}
