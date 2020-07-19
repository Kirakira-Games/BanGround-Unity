using AudioProvider;
using BGEditor;
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

    [SerializeField] private TextAsset[] voices;
    //sort
    private Text sort_Text;
    private Button sort_Button;

    public ISoundTrack previewSound { get; private set; }
    public FixBackground background { get; private set; }
    public DifficultySelect difficultySelect { get; private set; }

    static KVar cl_cursorter = new KVar("cl_cursorter", "1", KVarFlags.Archive, "Current sorter type", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    static KVar cl_lastsid = new KVar("cl_lastsid", "-1", KVarFlags.Archive, "Current chart set id", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    static Kommand cmd_playdemo = new Kommand("demo_play", "Play a demo file", async (args) =>
    {
        if(args.Length > 0)
        {
            if (SceneManager.GetActiveScene().name == "NewSelect")
            {
                var path = args[0];

                if(!File.Exists(path))
                {
                    if(KiraFilesystem.Instance.Exists(path))
                    {
                        path = Path.Combine(DataLoader.DataDir, path);
                    }
                    else
                    {
                        Debug.Log("[Demo Player] File not exists");
                        return;
                    }
                }

                var file = DemoFile.LoadFrom(path);

                var targetHeader = DataLoader.chartList.First(x => x.sid == file.sid);

                if(targetHeader == null)
                {
                    Debug.Log("[Demo Player] Target chartset not installed.");
                    return;
                }

                if(targetHeader.difficultyLevel[(int)file.difficulty] == -1)
                {
                    Debug.Log("[Demo Player] Target chart not installed.");
                    return;
                }

                LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == file.sid));
                LiveSetting.actualDifficulty = (int)file.difficulty;
                LiveSetting.currentDifficulty = (int)file.difficulty;

                LiveSetting.DemoFile = file;

                if (!await LiveSetting.LoadChart())
                {
                    return;
                }

                SceneLoader.LoadScene("NewSelect", "InGame", true);
            }
            else
            {
                Debug.Log("[Demo Player] Must use in select page!");
            }
        }
        else
        {
            Debug.Log("demo_play: Play a demo file<br />Usage: demo_play <demo file>");
        }
    });

    #region ChartEditor
    public void OpenMappingScene()
    {
        SceneLoader.LoadScene("NewSelect", "Mapping", true);
    }

    public async void ExportKiraPack()
    {
        var prevOrientation = Screen.orientation;
        if (Application.platform == RuntimePlatform.Android)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            await UniTask.DelayFrame(1);
        }
        var zip = DataLoader.BuildKiraPack(LiveSetting.CurrentHeader);
        var song = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        new NativeShare()
            .AddFile(zip)
            .SetSubject("Share " + song.title)
            .SetTitle("Share Kirapack")
            .SetText(song.title)
            .Share();
        if (Application.platform == RuntimePlatform.Android)
        {
            await UniTask.DelayFrame(1);
            Screen.orientation = prevOrientation;
        }
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
        GameObject.Find("UserInfo").GetComponent<UserInfo>().GetUserInfo();
        background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        PlayVoicesAtSceneIn();

        InitComponent();
        InitSort();

        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();
        SortSongList();

        m_srSongList.totalCount = -1;// DataLoader.chartList.Count;
        m_srSongList.RefillCells();

        Vector3[] vector3s = new Vector3[4];
        m_srSongList.gameObject.GetComponent<RectTransform>().GetWorldCorners(vector3s);
        m_flYMid = (vector3s[0].y + vector3s[1].y + vector3s[2].y + vector3s[3].y) * 0.5f;

        StartCoroutine(SelectDefaultCoroutine());
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

        currentSong = targetSong;
        if (lastSong != currentSong)
        {
            LiveSetting.currentChart = currentIndex;

            lastSong?.OnDeselect();
            currentSong.OnSelect();
            lastSong = currentSong;
            StartCoroutine(PreviewFadeOut(0.02f));

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
            if (success) SceneManager.LoadScene("NewSelect");
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
                previewSound?.Pause();
                yield return new WaitForSeconds(2.2f); //给语音留个地方
                previewSound?.Play();
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
            previewSound.SetVolume(i);
            yield return new WaitForFixedUpdate();
        }
    }

    private void PlayVoicesAtSceneIn()
    {
        AudioManager.Instance.PrecacheSE(voices[Random.Range(0, 3)].bytes).PlayOneShot();
    }

    private void PlayVoicesAtSceneOut()
    {
        AudioManager.Instance.PrecacheSE(voices[Random.Range(3, 7)].bytes).PlayOneShot();
    }

    #endregion

    private IEnumerator SelectDefaultCoroutine()
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
        cl_lastsid.Set(selectedSid);
        LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == selectedSid));

        //滚动到cl_lastsid的位置并选择
        yield return m_srSongList.ScrollToCell(LiveSetting.currentChart - 4, 9999);

        //Find targetSong
        var sis = m_tfContent.GetComponentsInChildren<SongItem>();
        float minDist = 10240;
        float dist;
        float yPos;
        RectTransform rt;
        Vector3[] childVector3s = new Vector3[4];
        for (int i = 0; i < sis.Length; i++)
        {
            if (sis[i].cHeader.sid == cl_lastsid)
            {
                rt = sis[i].transform as RectTransform;
                rt.GetWorldCorners(childVector3s);
                yPos = (childVector3s[0].y + childVector3s[1].y + childVector3s[2].y + childVector3s[3].y) * 0.5f;
                dist = Mathf.Abs(m_flYMid - yPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    currentSong = sis[i];
                }
            }
        }

        //Set Pos
        m_srSongList.StopMovement();
        Vector3[] v3s = new Vector3[4];
        var target = currentSong.gameObject.GetComponent<RectTransform>();
        target.GetWorldCorners(v3s);
        yPos = (v3s[0].y + v3s[1].y + v3s[2].y + v3s[3].y) * 0.5f;
        dist = m_flYMid - yPos;
        m_tfContent.anchoredPosition = m_tfContent.anchoredPosition + new Vector2(0, dist);

        //Select
        lastSong = currentSong;
        currentSong.OnSelect();
        StartCoroutine(PlayPreview());
    }

    private void InitComponent()
    {
        //sort
        sort_Button = GameObject.Find("Sort_Button").GetComponent<Button>();
        sort_Text = GameObject.Find("Sort_Text").GetComponent<Text>();
        sort_Button.onClick.AddListener(SwitchSort);

        difficultySelect = GameObject.Find("DifficultySelect").GetComponent<DifficultySelect>();
    }

    private void InitSort()
    {
        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
    }

    private void SwitchSort()
    {
        cl_cursorter.Set(cl_cursorter + 1);
        if (cl_cursorter > 4)
            cl_cursorter.Set(0);

        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
        RefeshSonglist();
    }

    private void SortSongList()
    {
        // Sort SongList
        IComparer<cHeader> compare;
        switch ((Sorter)cl_cursorter)
        {
            case Sorter.ChartDifficulty:
                compare = new ChartDifSort();
                break;
            case Sorter.SongName:
                compare = new SongNameSort();
                break;
            case Sorter.SongArtist:
                compare = new SongArtistSort();
                break;
            case Sorter.ChartAuthor:
                compare = new ChartAuthorSort();
                break;
            case Sorter.ChartScore:
                compare = new ChartScoreSort();
                break;
            default:
                compare = new SongNameSort();
                break;
        }
        DataLoader.chartList.Sort(compare);
    }

    public async void OnEnterPressed()
    {
        if (!await LiveSetting.LoadChart())
        {
            return;
        }
        StartCoroutine(PreviewFadeOut());
        SettingAndMod.instance.SetLiveSetting();

        KVSystem.Instance.SaveConfig();

        PlayVoicesAtSceneOut();
        SceneLoader.LoadScene("NewSelect", "InGame", true);
    }

    public void RefeshSonglist()
    {
        currentSong?.OnDeselect();
        SortSongList();
        m_srSongList.RefillCells();
        StartCoroutine(SelectDefaultCoroutine());
    }
}