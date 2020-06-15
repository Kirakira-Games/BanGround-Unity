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

    private Chart CreateChart(Difficulty difficulty, int level)
    {
        var chart = new Chart();
        chart.Difficulty = difficulty;
        chart.level = level;
        chart.notes.Add(new Note
        {
            type = NoteType.BPM,
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
        SceneManager.LoadScene("NewSelect");
    }

    public void NewChartSet()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            MessageBoxController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
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
        LiveSetting.currentDifficulty = difficulty;
        LiveSetting.actualDifficulty = difficulty;
        cl_lastsid.Set(header.sid);
        SceneManager.LoadScene("NewSelect");
    }

    public void NewDifficulty()
    {
        int difficulty = SelectedDifficulty();
        if (difficulty == -1)
        {
            MessageBoxController.ShowMsg(LogLevel.INFO, "Please select a difficulty.");
            return;
        }
        if (cHeader.difficultyLevel[difficulty] != -1)
        {
            MessageBoxController.ShowMsg(LogLevel.INFO, "This difficulty already exists.");
            return;
        }
        // Create chart
        int clamped = Mathf.Clamp(difficulty, 0, 3);
        int level = Random.Range(clamped * 5 + 5, clamped * 8 + 6);
        var chart = CreateChart((Difficulty) difficulty, level);
        DataLoader.SaveChart(chart, cHeader.sid, (Difficulty) difficulty);

        // Reload scene
        LiveSetting.currentDifficulty = difficulty;
        LiveSetting.actualDifficulty = difficulty;
        SceneManager.LoadScene("NewSelect");
    }

    public void 还没做好()
    {
        MessageBoxController.ShowMsg(LogLevel.INFO, "Coming soon!");
    }
}
