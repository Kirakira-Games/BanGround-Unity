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

    private SongItem lastSong = null;
    private SongItem currentSong = null;

    private void Awake()
    {
         
    }

    private void Start()
    {
        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();

        m_srSongList.totalCount = -1;// DataLoader.chartList.Count;
        m_srSongList.RefillCells();

    }

    private void LateUpdate()
    {
        if (!m_srSongList.m_Dragging && m_srSongList.needMove)
        {
            m_srSongList.needMove = false;

            //Scroll到合适位置
            var si = m_tfContent.GetComponentsInChildren<SongItem>();
            int mid = si.Count() / 2;
            m_srSongList.ScrollToCell(si[mid].idx - mid, 1000);

            //设置SongItem变大变小
            currentSong = si[mid];
            if (lastSong != currentSong)
            {
                lastSong?.OnDeselect();
                currentSong.OnSelect();
                lastSong = currentSong;
            }

            //更新当前选定的歌曲
            LiveSetting.currentChart = DataLoader.chartList.IndexOf(currentSong.cHeader);

            return;
        }

    }
}