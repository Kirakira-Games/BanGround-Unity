using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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

    private cHeader CreateHeader()
    {
        var header = new cHeader();
        header.version = ChartVersion;
        header.sid = DataLoader.GenerateSid();
        header.mid = cHeader.mid;
        if (UserInfo.username == null || UserInfo.username.Length == 0)
        {
            header.author = "Guest";
            header.authorNick = "Guest";
        }
        else
        {
            header.author = UserInfo.username;
            header.authorNick = UserInfo.result.nickname;
        }
        header.backgroundFile = cHeader.backgroundFile;
        header.preview = cHeader.preview.ToArray();
        header.tag = cHeader.tag.ToList();
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

    public void 还没做好()
    {
        MessageBannerController.ShowMsg(LogLevel.INFO, "Coming soon!");
    }
}
