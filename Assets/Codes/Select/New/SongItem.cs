using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongItem : MonoBehaviour
{
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
        songName.text = idx.ToString();
        gameObject.name = idx.ToString();
        Debug.Log(idx);
    }
}
