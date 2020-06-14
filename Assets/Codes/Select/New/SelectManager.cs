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

    private float m_flYMid;
    private int m_iSelectedItem;
    private bool m_bDirty = false;

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

        Vector3[] vector3s = new Vector3[4];
        m_srSongList.gameObject.GetComponent<RectTransform>().GetWorldCorners(vector3s);
        m_flYMid = (vector3s[0].y + vector3s[1].y + vector3s[2].y + vector3s[3].y) * 0.5f;
    }

    private void LateUpdate()
    {
        if (!m_srSongList.m_Dragging)
        {
            if (!m_bDirty)
                return;

            m_srSongList.StopMovement();

            Vector3[] v3s = new Vector3[4];

            var target = currentSong.gameObject.GetComponent<RectTransform>();
            target.GetWorldCorners(v3s);

            var yPos = (v3s[0].y + v3s[1].y + v3s[2].y + v3s[3].y) * 0.5f;

            float dist = m_flYMid - yPos;

            m_tfContent.anchoredPosition = m_tfContent.anchoredPosition + new Vector2(0, dist);

            m_bDirty = false;
            return;
        }

        m_bDirty = true;

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

        if (currentIndex == LiveSetting.currentChart)
            return;

        var sis = m_tfContent.GetComponentsInChildren<SongItem>();

        for (int i = 0; i < sis.Length; i++)
        {
            if (sis[i] == targetSong)
            {
                m_iSelectedItem = i;
                break;
            }
        }

        currentSong = sis[m_iSelectedItem];
        if (lastSong != currentSong)
        {
            lastSong?.OnDeselect();
            currentSong.OnSelect();
            lastSong = currentSong;
        }

        LiveSetting.currentChart = currentIndex;
    }
}