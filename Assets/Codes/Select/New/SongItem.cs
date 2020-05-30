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

    public RectTransform rectTransform = null;
    public bool isDummy = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        
    }

    void ScrollCellIndex(int idx)
    {
        if(idx == 0)
        {
            var trans = new Color(0, 0, 0, 0);

            songName.color = trans;
            songArtist.color = trans;
            GetComponent<Image>().color = trans;

            isDummy = true;
        }
        else
        {
            cHeader = DataLoader.chartList[idx - 1];
            mHeader = DataLoader.GetMusicHeader(cHeader.mid);

            songName.text = mHeader.title;
            songArtist.text = mHeader.artist;
        }
    }
}
