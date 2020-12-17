using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Zenject;
using BanGround;
using BanGround.Scene.Params;

#pragma warning disable 0649
public class LoaderInfo : MonoBehaviour
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartLoader chartLoader;
    [Inject]
    private IFileSystem fs;

    private mHeader musicHeader;
    private cHeader chartHeader;
    private InGameParams parameters;

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
        parameters = SceneLoader.GetParamsOrDefault<InGameParams>();
        chartHeader = dataLoader.GetChartHeader(parameters.sid);
        chartHeader.LoadDifficultyLevels(dataLoader);
        musicHeader = dataLoader.GetMusicHeader(chartHeader.mid);
    }

    private void AppendChartInfo(bool success)
    {
        if (!success) return;
        songBPM.text = GetBPM() + " NOTE " + chartLoader.gameChart.numNotes;
    }

    private void ShowInfo()
    {
        // Song img
        var path = dataLoader.GetBackgroundPath(chartHeader.sid).Item1;
        if (path != null && fs.FileExists(path))
        {
            var tex = fs.GetFile(path).ReadAsTexture();
            songImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        // Song name
        songName.text = string.Format(NameFormat, musicHeader.title);

        // BPM
        songBPM.text = "Loading...";
        
        // Difficulty and charter
        Difficulty difficulty = parameters.difficulty;
        int level = chartHeader.difficultyLevel[(int)difficulty];
        songLevelAndCharter.text = string.Format(LevelAndCharterFormat, difficulty.ToString().ToUpper(), level, chartHeader.authorNick);
        
        // Artist
        songArtist.text = string.Format(ArtistFormat, musicHeader.artist);
    }

    private string GetBPM()
    {
        var min = chartLoader.gameChart.bpm.Min(o => o.value);
        var max = chartLoader.gameChart.bpm.Max(o => o.value);
        return min == max ? $"BPM {min}" : $"BPM {min}-{max}";
    }
}
