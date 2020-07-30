using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UniRx.Async;
using System.IO;
using NVorbis;
using System.Text;
using System.Collections.Generic;
using System.Threading;
public class ChartCreator : MonoBehaviour
{
    public const int ChartVersion = 1;
    public Button Blocker;
    public Toggle[] Toggles;

    private cHeader cHeader => LiveSetting.CurrentHeader;
    private static KVarRef cl_lastsid = new KVarRef("cl_lastsid");

    public void Show()
    {
        Blocker.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Toggles[LiveSetting.actualDifficulty].isOn = true;
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
            mid = DataLoader.GenerateMid(),
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
        header.sid = DataLoader.GenerateSid();
        header.mid = mid == -1 ? cHeader.mid : mid;
        if (UserInfo.username == null || UserInfo.username.Length == 0)
        {
            header.author = "Guest";
            header.authorNick = "Guest";
        }
        else
        {
            header.author = UserInfo.username;
            header.authorNick = Authenticate.user.Nickname;
        }
        header.backgroundFile = cHeader.backgroundFile;
        header.preview = cHeader.preview == null ? new float[] { 0.0f, 0.0f } : cHeader.preview.ToArray();
        header.tag = cHeader.tag == null ? new List<string>() : cHeader.tag.ToList();
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
        DataLoader.DuplicateKiraPack(cHeader);
        SceneManager.LoadScene("Select");
    }

    public void NewChartSet()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            MessageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }
        // Create header
        var header = CreateHeader();
        DataLoader.SaveHeader(header);

        // Create chart
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var chart = CreateChart((Difficulty) difficulty, level);
        DataLoader.SaveChart(chart, header.sid, (Difficulty) difficulty);

        // Reload scene
        LiveSetting.currentDifficulty.Set(difficulty);
        LiveSetting.actualDifficulty = difficulty;
        cl_lastsid.Set(header.sid);
        SceneManager.LoadScene("Select");
    }

    public void NewDifficulty()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            MessageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }
        if (cHeader.difficultyLevel[difficulty] != -1)
        {
            MessageBannerController.ShowMsg(LogLevel.INFO, "This difficulty already exists.");
            return;
        }
        // Create chart
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var chart = CreateChart((Difficulty) difficulty, level);
        DataLoader.SaveChart(chart, cHeader.sid, (Difficulty) difficulty);

        // Reload scene
        LiveSetting.currentDifficulty.Set(difficulty);
        LiveSetting.actualDifficulty = difficulty;
        SceneManager.LoadScene("Select");
    }

    public static bool RequestAirdrop = false;
    public static byte[] AirdroppedFile = null;

    public async void ImportMusic()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            MessageBannerController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        var task = WaitForAirdrop(tokenSource.Token);

        LoadingBlocker.instance.Show("Waiting for airdrop (You must drop a ogg music!!!)...", () =>
        {
            tokenSource.Cancel();
        });

        await task;

        LoadingBlocker.instance.Close();
    }
    public async UniTask WaitForAirdrop(CancellationToken token = default)
    {
        try
        {
            int difficulty = SelectedDifficulty();

            RequestAirdrop = true;

            await UniTask.Delay(1500, default, default, token);

            Application.OpenURL("http://127.0.0.1:8088/");

            await UniTask.WaitUntil(() => AirdroppedFile != null, default, token);

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
                        MessageBannerController.ShowMsg(LogLevel.ERROR, "YOU MUST DROP A PROPPER OGG FILE TO CONTINUE!!!");

                        return;
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
            DataLoader.SaveHeader(mheader, AirdroppedFile);

            // Create header
            var header = CreateHeader(mheader.mid);
            DataLoader.SaveHeader(header);

            // Create chart
            int clamped = Mathf.Clamp(difficulty, 0, 3);
            int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
            var chart = CreateChart((Difficulty)difficulty, level);
            DataLoader.SaveChart(chart, header.sid, (Difficulty)difficulty);

            // Reload scene
            LiveSetting.currentDifficulty.Set(difficulty);
            LiveSetting.actualDifficulty = difficulty;
            cl_lastsid.Set(header.sid);
            SceneManager.LoadScene("Select");

            AirdroppedFile = null;
        }
        catch (System.OperationCanceledException)
        {
            MessageBannerController.ShowMsg(LogLevel.INFO, "Canceled");

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
        MessageBannerController.ShowMsg(LogLevel.INFO, "Coming soon!");
    }
}
