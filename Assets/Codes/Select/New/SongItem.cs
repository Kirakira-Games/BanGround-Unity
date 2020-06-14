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

    public int idx;
    public int id;

    void ScrollCellIndex(int idx)
    {
        this.idx = idx;
        id = FlatID(idx);
        cHeader = DataLoader.chartList[id];
        mHeader = DataLoader.GetMusicHeader(cHeader.mid);
        name = id.ToString();
        songName.text = mHeader.title;
        songArtist.text = mHeader.artist;
    }

    public void OnSelect()
    {
        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = selectedSize;
    }

    public void OnDeselect()
    {
        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = deselectedSize;
    }

    int FlatID(int idx)
    {
        int count = DataLoader.chartList.Count;
        int mod = idx % count;
        return mod < 0 ? count + mod : mod;
    }
}
