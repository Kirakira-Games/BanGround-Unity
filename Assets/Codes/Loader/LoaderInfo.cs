using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Zenject;

#pragma warning disable 0649
public class LoaderInfo : MonoBehaviour
{
    [Inject]
    private IDataLoader dataLoader;

    private mHeader musicHeader;
    private cHeader chartHeader;

    [SerializeField] private Image songImg;
    [SerializeField] private Text songName;
    [SerializeField] private Text songBPM;
    [SerializeField] private Text songLevelAndCharter;
    [SerializeField] private Text songArtist;

    const string NameFormat = "{0}";
    const string LevelAndCharterFormat = "{0} {1}\n{2}"; 
    const string ArtistFormat = "{0}";

    private void Start()
    {
        SceneLoader.onTaskFinish.AddListener(AppendChartInfo);
        GetInfo();
        ShowInfo();
    }

    private void GetInfo()
    {
        chartHeader = LiveSetting.CurrentHeader;
        musicHeader = dataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
    }

    private void AppendChartInfo(bool success)
    {
        if (!success) return;
        songBPM.text = GetBPM() + " NOTE " + LiveSetting.gameChart.numNotes;
    }

    private void ShowInfo()
    {
        // Song img
        var path = dataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        if (path != null && KiraFilesystem.Instance.Exists(path))
        {
            var tex = KiraFilesystem.Instance.ReadTexture2D(path);
            songImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        // Song name
        songName.text = string.Format(NameFormat, musicHeader.title);

        // BPM
        songBPM.text = "Loading...";
        
        // Difficulty and charter
        Difficulty difficulty = (Difficulty)LiveSetting.actualDifficulty;
        int level = chartHeader.difficultyLevel[(int)difficulty];
        songLevelAndCharter.text = string.Format(LevelAndCharterFormat, difficulty.ToString().ToUpper(), level, chartHeader.authorNick);
        
        // Artist
        songArtist.text = string.Format(ArtistFormat, musicHeader.artist);
    }

    private string GetBPM()
    {
        var min = LiveSetting.gameChart.bpm.Min(o => o.value);
        var max = LiveSetting.gameChart.bpm.Max(o => o.value);
        return min == max ? $"BPM {min}" : $"BPM {min}-{max}";
    }
}
