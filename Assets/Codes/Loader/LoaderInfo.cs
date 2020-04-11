using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] private Text songNoteCount;

    const string NameFormat = "{0}";// Offset Rhythm Test
    const string BPMFormat = "BPM {0}-{1}";// BPM 13-269
    const string LevelAndCharterFormat = "{0} {1} BY {2}"; // EASY 26 BY User
    const string ArtistFormat = "Produced by {0}";// Produced by KIRAKIRA sound team "bbben"
    const string NoteCountFormat = "Note {0}";//Note 999

    void Start()
    {
        //Get Info
        chartHeader = LiveSetting.CurrentHeader;
        musicHeader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        chart = DataLoader.LoadChart(LiveSetting.CurrentHeader.sid, (Difficulty)LiveSetting.actualDifficulty);
        chartData = ChartLoader.LoadChart(chart);

        //Show Info
        var tex = KiraFilesystem.Instance.ReadTexture2D(DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1);
        songImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        songName.text = string.Format(NameFormat, musicHeader.title);
        songBPM.text = string.Format(BPMFormat, chartData.bpm.Min().value, chartData.bpm.Max().value);
        songLevelAndCharter.text = string.Format(LevelAndCharterFormat, Enum.GetName(typeof(Difficulty), chart.Difficulty).ToUpper(), chart.level, chartHeader.authorNick);
        songArtist.text = string.Format(ArtistFormat, musicHeader.artist);
        songNoteCount.text = string.Format(NoteCountFormat, chartData.notes.Count);
    }

}
