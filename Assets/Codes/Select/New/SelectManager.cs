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

    private float m_flYMid;

    private SongItem m_siDummy;

    private Vector2 test;

    private void Awake()
    {
         
    }

    private void Start()
    {
        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();

        m_srSongList.totalCount = DataLoader.chartList.Count + 1;
        m_srSongList.RefillCells();

        Vector3[] vector3s = new Vector3[4];
        m_srSongList.gameObject.GetComponent<RectTransform>().GetWorldCorners(vector3s);
        m_flYMid = (vector3s[0].y + vector3s[1].y + vector3s[2].y + vector3s[3].y) * 0.5f;

        test = new Vector2(0, m_srSongList.gameObject.GetComponent<RectTransform>().rect.height / 4 - m_tfContent.rect.height);

        foreach (RectTransform child in m_tfContent)
        {
            var si = child.GetComponent<SongItem>();
            if (si.isDummy)
            {
                m_siDummy = si;
                break;
            }
        }
    }

    private void Update()
    {
        if(m_siDummy.rectTransform != null)
        {
            m_siDummy.rectTransform.sizeDelta = test;
        }
    }

    private void LateUpdate()
    {
        float minDist = 10240;

        SongItem targetSong = null;

        Vector3[] childVector3s = new Vector3[4];

        foreach (RectTransform child in m_tfContent)
        {
            child.GetWorldCorners(childVector3s);

            var yPos = (childVector3s[0].y + childVector3s[1].y + childVector3s[2].y + childVector3s[3].y) * 0.5f;

            float dist = Mathf.Abs(m_flYMid - yPos);
            if (dist < minDist)
            {
                minDist = dist;
                targetSong = child.gameObject.GetComponent<SongItem>();
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