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

    public RectTransform m_tfDummyStart;
    public RectTransform m_tfDummyEnd;

    private int m_iSelectedItem;

    private float m_flYMid;

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

        Vector3[] vector3s = new Vector3[4];
        m_srSongList.gameObject.GetComponent<RectTransform>().GetWorldCorners(vector3s);
        m_flYMid = (vector3s[0].y + vector3s[1].y + vector3s[2].y + vector3s[3].y) * 0.5f;

        // Dirty hack to allow songitems moving around while there's only few somg
        
        // Move dummy end to end
        m_tfDummyEnd.SetParent(null);
        m_tfDummyEnd.SetParent(m_tfContent);

        // Set dummy height
        var dummyHeight = new Vector2(0, Mathf.Min(m_srSongList.gameObject.GetComponent<RectTransform>().rect.height / 2 - 120));
        m_tfDummyStart.sizeDelta = dummyHeight;
        m_tfDummyEnd.sizeDelta = dummyHeight;
    }

    private bool firstUpdate = true;

    private void LateUpdate()
    {
        if(!m_srSongList.m_Dragging)
        {
            m_srSongList.StopMovement();

            m_tfContent.anchoredPosition = new Vector2(0, 160 * m_iSelectedItem);

            return;
        }

        float minDist = 10240;

        SongItem targetSong = null;

        Vector3[] childVector3s = new Vector3[4];

        foreach (RectTransform child in m_tfContent)
        {
            var si = child.gameObject.GetComponent<SongItem>();

            if (si == null)
                continue;

            child.GetWorldCorners(childVector3s);

            var yPos = (childVector3s[0].y + childVector3s[1].y + childVector3s[2].y + childVector3s[3].y) * 0.5f;

            float dist = Mathf.Abs(m_flYMid - yPos);
            if (dist < minDist)
            {
                minDist = dist;
                targetSong = si;
            }
        }

        if (targetSong == null)
            return;

        int currentIndex = DataLoader.chartList.IndexOf(targetSong.cHeader);

        if (currentIndex == LiveSetting.currentChart && !firstUpdate)
            return;

        firstUpdate = false;

        for(int i = 1; i < m_tfContent.childCount - 1; i++)
        {
            if(m_tfContent.GetChild(i).GetComponent<SongItem>() == targetSong)
            {
                m_iSelectedItem = i - 1;
                break;
            }
        }

        LiveSetting.currentChart = currentIndex;

        m_txtTitle.text = targetSong.mHeader.title;
        m_txtArtist.text = targetSong.mHeader.artist;
        m_txtCharter.text = targetSong.cHeader.author;
    }
}