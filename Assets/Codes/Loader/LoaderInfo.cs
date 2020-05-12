using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

#pragma warning disable 0649
public class LoaderInfo : MonoBehaviour
{
    private mHeader musicHeader;
    private cHeader chartHeader;
    private Chart chart;
    private GameChartData chartData;

    [SerializeField] private Image songImg;
    [SerializeField] private Text songName;
    [SerializeField] private Text songBPM;
    [SerializeField] private Text songLevelAndCharter;
    [SerializeField] private Text songArtist;

    const string NameFormat = "{0}";
    const string LevelAndCharterFormat = "{0}\n{1}"; 
    const string ArtistFormat = "{0}";

    private void Start()
    {
        GetInfo();
        ShowInfo();
    }

    private void GetInfo()
    {
        chartHeader = LiveSetting.CurrentHeader;
        musicHeader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        chart = DataLoader.LoadChart(LiveSetting.CurrentHeader.sid, (Difficulty)LiveSetting.actualDifficulty);
        chartData = ChartLoader.LoadChart(chart);
    }

    private void ShowInfo()
    {
        //Show Info
        var path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        if (path != null && KiraFilesystem.Instance.Exists(path))
        {
            var tex = KiraFilesystem.Instance.ReadTexture2D(path);
            songImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        songName.text = string.Format(NameFormat, musicHeader.title);
        songBPM.text = GetBPM() + " NOTE " + chartData.numNotes;
        songLevelAndCharter.text = string.Format(LevelAndCharterFormat, Enum.GetName(typeof(Difficulty), chart.Difficulty).ToUpper(), chart.level, chartHeader.authorNick);
        songArtist.text = string.Format(ArtistFormat, musicHeader.artist);
    }

    private string GetBPM()
    {
        var min = chartData.bpm.Min(o => o.value);
        var max = chartData.bpm.Max(o => o.value);
        return min == max ? $"BPM{min}" : $"BPM{min}-{max}";
    }
}
