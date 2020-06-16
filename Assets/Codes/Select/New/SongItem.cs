using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class SongItem : MonoBehaviour
{
    public cHeader cHeader;
    public mHeader mHeader;

    public Text songName;
    public Text songArtist;

    private static readonly Vector2 selectedSize = new Vector2(888, 180);
    private static readonly Vector2 deselectedSize = new Vector2(888, 120);
    private static readonly Color selectedColor = new Color(1, 1, 1);
    private static readonly Color deselectedColor = new Color(137f / 255, 137f / 255, 137f / 255);

    public int idx;
    public int id;

    private Button Enter_Btn;

    void ScrollCellIndex(int idx)
    {
        this.idx = idx;
        id = FlatID(idx);
        cHeader = DataLoader.chartList[id];
        mHeader = DataLoader.GetMusicHeader(cHeader.mid);
        name = idx.ToString();
        songName.text = mHeader.title;
        songArtist.text = mHeader.artist;
        songArtist.color = selectedColor;
        Enter_Btn = GetComponent<Button>();
    }

    public void OnSelect()
    {
        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = selectedSize;
        songName.color = selectedColor;

        var path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        SelectManager.instance.background.UpdateBackground(path);

        SelectManager.instance.difficultySelect.levels = LiveSetting.CurrentHeader.difficultyLevel.ToArray();
        SelectManager.instance.difficultySelect.OnSongChange();

        Enter_Btn.onClick.AddListener(() => SelectManager.instance.OnEnterPressed());
    }

    public void OnDeselect()
    {
        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = deselectedSize;
        songName.color = deselectedColor;

        Enter_Btn.onClick.RemoveAllListeners();
    }

    int FlatID(int idx)
    {
        int count = DataLoader.chartList.Count;
        int mod = idx % count;
        return mod < 0 ? count + mod : mod;
    }
}
