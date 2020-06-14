using AudioProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SelectManager : MonoBehaviour
{
    public static SelectManager instance { get; private set; }

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

    public ISoundTrack previewSound { get; private set; }

    static KVar cl_cursorter = new KVar("cl_cursorter", "1", KVarFlags.Archive, "Current sorter type", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    static KVar cl_lastsid = new KVar("cl_lastsid", "-1", KVarFlags.Archive, "Current chart set id", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    #region ChartEditor
    public void OpenMappingScene()
    {
        SceneLoader.LoadScene("NewSelect", "Mapping", true);
    }

    public async void ExportKiraPack()
    {
        var prevOrientation = Screen.orientation;
        Screen.orientation = ScreenOrientation.Portrait;
        await UniTask.DelayFrame(1);
        var zip = DataLoader.BuildKiraPack(LiveSetting.CurrentHeader);
        var song = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        new NativeShare()
            .AddFile(zip)
            .SetSubject("Share " + song.title)
            .SetTitle("Share Kirapack")
            .SetText(song.title)
            .Share();
        await UniTask.DelayFrame(1);
        Screen.orientation = prevOrientation;
    }

    public void DuplicateKiraPack()
    {
        DataLoader.DuplicateKiraPack(LiveSetting.CurrentHeader);
        SceneManager.LoadScene("NewSelect");
    }
    #endregion

    #region Unity Function
    private void Awake()
    {
        instance = this;
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

        SelectDefault();
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

        currentSong = targetSong;// sis[m_iSelectedItem];
        if (lastSong != currentSong)
        {
            lastSong?.OnDeselect();
            currentSong.OnSelect();
            lastSong = currentSong;

            LiveSetting.currentChart = currentIndex;
            cl_lastsid.Set(currentSong.cHeader.sid);
            StopCoroutine(PlayPreview());
            StartCoroutine(PlayPreview());
        }

        
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            previewSound?.Pause();
        else
            previewSound?.Play();
    }

#if UNITY_EDITOR
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            bool success = DataLoader.LoadAllKiraPackFromInbox();
            if (success) SceneManager.LoadScene("Select");
        }
    }
#endif

    private void OnDestroy()
    {
        previewSound?.Dispose();
        SlideMesh.cacheMat = null;
    }
    #endregion

    #region Music Preview
    bool isFirstPlay = true;
    IEnumerator PlayPreview()
    {
        mHeader mheader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        if (previewSound != null)
        {
            previewSound.Dispose();
            previewSound = null;
        }
        if (DataLoader.MusicExists(LiveSetting.CurrentHeader.mid))
        {
            previewSound = AudioManager.Instance.PlayLoopMusic(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), true,
                new uint[]
                {
                (uint)(mheader.preview[0] * 1000),
                (uint)(mheader.preview[1] * 1000)
                    },
                    false);
            if (isFirstPlay)
            {
                previewSound.Pause();
                yield return new WaitForSeconds(2.2f); //给语音留个地方
                previewSound.Play();
            }
            StopCoroutine("PreviewFadeIn");
            StartCoroutine(PreviewFadeIn());
            isFirstPlay = false;
        }
    }

    IEnumerator PreviewFadeOut(float speed = 0.008F)
    {
        if (previewSound == null) yield break;
        for (float i = 0.7f; i > 0; i -= speed)
        {
            StopCoroutine("PreviewFadeIn");
            StopCoroutine("PreviewFadeOut");
            //print("FadingOut");
            previewSound.SetVolume(i);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator PreviewFadeIn()
    {
        if (previewSound == null) yield break;
        for (float i = 0f; i < 0.7f; i += 0.02f)
        {
            StopCoroutine("PreviewFadeIn");
            StopCoroutine("PreviewFadeOut");
            //print("FadingIn");
            previewSound.SetVolume(i);
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion

    private void SelectDefault()
    {
        int selectedSid = cl_lastsid;
        // After import, select imported chart
        if (DataLoader.LastImportedSid != -1)
        {
            selectedSid = DataLoader.LastImportedSid;
            DataLoader.LastImportedSid = -1;
        }
        if (selectedSid == -1 || DataLoader.chartList.Find(item => item.sid == selectedSid) == null)
        {
            selectedSid = DataLoader.chartList[Random.Range(0, DataLoader.chartList.Count)].sid;
        }
        LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == selectedSid));

        //TODO:
        //滚动到cl_lastsid的位置并选择
        m_srSongList.ScrollToCell(LiveSetting.currentChart, float.MaxValue);
        //m_bDirty = true;
    }

}