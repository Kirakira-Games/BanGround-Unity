using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongItem : MonoBehaviour
{
    public cHeader cHeader;
    public mHeader mHeader;

    public Text songName;
    public Text songArtist;

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
}
