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
        songBPM.text = GetBPM() + " NOTE " + LiveSetting.gameChart.numNotes;
        songLevelAndCharter.text = string.Format(LevelAndCharterFormat, LiveSetting.chart.difficulty.ToString().ToUpper(), LiveSetting.chart.level, chartHeader.authorNick);
        songArtist.text = string.Format(ArtistFormat, musicHeader.artist);
    }

    private string GetBPM()
    {
        var min = LiveSetting.gameChart.bpm.Min(o => o.value);
        var max = LiveSetting.gameChart.bpm.Max(o => o.value);
        return min == max ? $"BPM {min}" : $"BPM {min}-{max}";
    }
}
