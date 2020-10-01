using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.IO;
using NVorbis;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Zenject;

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
    private cHeader CreateHeader(int mid = -1)
    {
        var header = new cHeader();
        header.version = ChartVersion;
        header.sid = dataLoader.GenerateSid();
        header.mid = mid == -1 ? cHeader.mid : mid;
        bool copyInfo = header.mid == cHeader.mid;
        if (UserInfo.isOffline)
        {
            header.author = "Guest";
            header.authorNick = "Guest";
        }
        else
        {
            header.author = UserInfo.user.Username;
            header.authorNick = UserInfo.user.Nickname;
        }
        header.backgroundFile = cHeader.backgroundFile;
        header.preview = (!copyInfo || cHeader.preview == null) ? new float[] { 0.0f, 0.0f } : cHeader.preview.ToArray();
        header.tag = (!copyInfo || cHeader.tag == null) ? new List<string>() : cHeader.tag.ToList();
        return header;
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
        var chart = CreateChart((Difficulty) difficulty, level);
        dataLoader.SaveChart(chart, header.sid, (Difficulty) difficulty);

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
        var chart = CreateChart((Difficulty) difficulty, level);
        dataLoader.SaveChart(chart, cHeader.sid, (Difficulty) difficulty);

        // Reload scene
        cl_lastdiff.Set(difficulty);
        chartListManager.current.difficulty = (Difficulty) difficulty;
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

            RequestAirdrop = true;

            await UniTask.Delay(1500).WithCancellation(token);

            Application.OpenURL("http://127.0.0.1:8088/");

            await UniTask.WaitUntil(() => AirdroppedFile != null).WithCancellation(token);

            RequestAirdrop = false;

            var title = "New Song";
            var artist = "Unknown Artist";
            var len = -1.0f;

            using (var stream = new MemoryStream(AirdroppedFile))
            {
                // check airdropped file
                using (var br = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (new string(br.ReadChars(4)) != "OggS")
                    {
                        messageBannerController.ShowMsg(LogLevel.ERROR, "YOU MUST DROP A PROPPER OGG FILE TO CONTINUE!!!");

                        throw new InvalidDataException("YOU MUST DROP A PROPPER OGG FILE TO CONTINUE!!!");
                    }
                }

                // get infomation from file
                using (var reader = new VorbisReader(stream))
                {
                    title = reader.Tags.Title == string.Empty ? "New Song" : reader.Tags.Title;
                    artist = reader.Tags.Artist == string.Empty ? "Unknown Artist" : reader.Tags.Artist;
                    len = (float)reader.TotalTime.TotalSeconds;
                }
            }

            // Create mheader
            var mheader = CreateMHeader(title, artist, len);
            dataLoader.SaveHeader(mheader, AirdroppedFile);

            // Create header
            var header = CreateHeader(mheader.mid);
            dataLoader.SaveHeader(header);

            // Create chart
            int clamped = Mathf.Clamp(difficulty, 0, 3);
            int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
            var chart = CreateChart((Difficulty)difficulty, level);
            dataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

            // Reload scene
            cl_lastdiff.Set(difficulty);
            chartListManager.current.difficulty = (Difficulty) difficulty;
            cl_lastsid.Set(header.sid);
            SceneManager.LoadScene("Select");

            AirdroppedFile = null;
        }
        catch (System.OperationCanceledException)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Canceled");

            RequestAirdrop = false;
            AirdroppedFile = null;
        }
        catch (System.Exception ex)
        {
            RequestAirdrop = false;
            AirdroppedFile = null;

            throw ex;
        }
    }

    public void 还没做好()
    {
        messageBannerController.ShowMsg(LogLevel.INFO, "Coming soon!");
    }
}
