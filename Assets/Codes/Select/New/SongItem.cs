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

    void Start()
    {
    }

    void Update()
    {
        
    }

    void ScrollCellIndex(int idx)
    {
        cHeader = DataLoader.chartList[idx];
        mHeader = DataLoader.GetMusicHeader(cHeader.mid);

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
}
