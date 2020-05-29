using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    public LoopVerticalScrollRect m_srSongList;
    public RectTransform m_tfContent;
    
    public Text m_txtTitle;
    public Text m_txtArtist;
    public Text m_txtCharter;

    private void Awake()
    {
         
    }

    private void Start()
    {
        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();

        m_srSongList.totalCount = DataLoader.chartList.Count;
        m_srSongList.RefillCells();
    }

    private void LateUpdate()
    {
        float minDist = 10240;
        float minDistDelta = 10240;

        SongItem targetSong = null;

        float Ymid = m_tfContent.rect.height / 12;

        foreach(RectTransform child in m_tfContent)
        {
            float dist = Math.Abs(Ymid - child.rect.y);
            float delta = minDist - dist;
            if (delta < minDistDelta)
            {
                minDistDelta = delta;
                minDist = dist;

                targetSong = child.gameObject.GetComponent<SongItem>();
            }
            else
            {
                break;
            }
        }

        int currentIndex = DataLoader.chartList.IndexOf(targetSong.cHeader);

        if (currentIndex == LiveSetting.currentChart)
            return;

        LiveSetting.currentChart = currentIndex;

        m_txtTitle.text = targetSong.mHeader.title;
        m_txtArtist.text = targetSong.mHeader.artist;
        m_txtCharter.text = targetSong.cHeader.author;
    }
}