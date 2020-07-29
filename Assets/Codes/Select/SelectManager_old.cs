using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;
using AudioProvider;
using Random = UnityEngine.Random;
using UniRx.Async;
using UnityEngine.Profiling;

#pragma warning disable 0649
public class SelectManager_old : MonoBehaviour
{
    public const float scroll_Min_Speed = 50f;

    private cHeader lastcHeader = new cHeader();

    RectTransform rt;
    ScrollRect rt_s;
    VerticalLayoutGroup lg;
    public DragHandler dh;

    //sort
    private Text sort_Text;
    private Button sort_Button;

    public GameObject songItemPrefab;

    [SerializeField] 
    private TextAsset[] voices;

    public List<cHeader> chartList => DataLoader.chartList;
    List<GameObject> SelectButtons = new List<GameObject>();
    List<RectControl> rcs = new List<RectControl>();

    DifficultySelect difficultySelect;

    public static SelectManager_old instance;

    [HideInInspector] 
    public ISoundTrack previewSound;

    static KVar cl_cursorter = new KVar("cl_cursorter", "1", KVarFlags.Archive, "Current sorter type", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    static KVar cl_lastsid = new KVar("cl_lastsid", "-1", KVarFlags.Archive, "Current chart set id", obj =>
    {
        KVSystem.Instance.SaveConfig();
    });

    static Kommand cmd_playdemo = new Kommand("demo_play", "Play a demo file", args =>
    {
        if (args.Length > 0)
        {
            if (SceneManager.GetActiveScene().name == "Select")
            {
                var path = args[0];

                if (!File.Exists(path))
                {
                    if (KiraFilesystem.Instance.Exists(path))
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

                if (targetHeader == null)
                {
                    Debug.Log("[Demo Player] Target chartset not installed.");
                    return;
                }

                if (targetHeader.difficultyLevel[(int)file.difficulty] == -1)
                {
                    Debug.Log("[Demo Player] Target chart not installed.");
                    return;
                }

                LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == file.sid));
                LiveSetting.actualDifficulty = (int)file.difficulty;
                LiveSetting.currentDifficulty.Set((int)file.difficulty);

                LiveSetting.DemoFile = file;

                //if (!await LiveSetting.LoadChart(true))
                //{
                //    return;
                //}

                SceneLoader.LoadScene("Select", "InGame", () => LiveSetting.LoadChart(true));
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


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        GameObject.Find("UserInfo").GetComponent<UserInfo>().GetUserInfo();


        int selectedSid = cl_lastsid;

        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();

        InitComponent();
        InitSort();
        InitSongList(selectedSid);
        PlayVoicesAtSceneIn();
    }

    private async void PlayVoicesAtSceneIn()
    {
        (await AudioManager.Instance.PrecacheSE(voices[Random.Range(0, 3)].bytes)).PlayOneShot();
    }

    private async void PlayVoicesAtSceneOut()
    {
        (await AudioManager.Instance.PrecacheSE(voices[Random.Range(3, 7)].bytes)).PlayOneShot();
    }

    private void InitComponent()
    {
        //sort
        sort_Button = GameObject.Find("Sort_Button").GetComponent<Button>();
        sort_Text = GameObject.Find("Sort_Text").GetComponent<Text>();
        sort_Button.onClick.AddListener(SwitchSort);

        //Main Scroll View
        rt = GameObject.Find("SongContent").GetComponent<RectTransform>();
        rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        dh = GameObject.Find("Song Scroll View").GetComponent<DragHandler>();
        lg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();

        difficultySelect = GameObject.Find("DifficultySelect").GetComponent<DifficultySelect>();
    }

    //---------------------------------------------
    void SwitchSort()
    {
        cl_cursorter.Set(cl_cursorter + 1);
        if (cl_cursorter > 4) 
            cl_cursorter.Set(0);

        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
        InitSongList(LiveSetting.CurrentHeader.sid);
    }

    void InitSort()
    {
        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
    }

    //Song Selection-------------------------------
    public async void InitSongList(int selectedSid = -1)
    {
        lg.enabled = true;
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
        chartList.Sort(compare);

        //Remove Old SongItem
        for (int i = SelectButtons.Count - 1; i >= 0; i--) 
        {
            Destroy(SelectButtons[i].gameObject);
        }
        SelectButtons.Clear();
        rcs.Clear();
        last = null;

        Transform pos = GameObject.Find("SongContent").transform;
        //Spawn New SongItem
        for (int i = 0; i < chartList.Count; i++)
        {
            GameObject go = Instantiate(songItemPrefab, pos);
            go.name = i.ToString();
            Text[] txt = go.GetComponentsInChildren<Text>();

            cHeader chart = chartList[i];
            mHeader song = DataLoader.GetMusicHeader(chart.mid);
            string author = chart.authorNick;
            txt[0].text = song.title;
            txt[1].text = song.artist + " / " + author;
            var rc = go.GetComponent<RectControl>();
            rc.index = i;
            rcs.Add(rc);
            SelectButtons.Add(go);
        }

        //lg.padding = new RectOffset(0, 0, (int)((rt_v.sizeDelta.y / 2) - 100), 0);

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, lg.padding.top * 2 + chartList.Count * (116) + (chartList.Count - 1) * lg.spacing + (800));

        // After import, select imported chart
        if (DataLoader.LastImportedSid != -1)
        {
            selectedSid = DataLoader.LastImportedSid;
            DataLoader.LastImportedSid = -1;
        }
        if (selectedSid != -1)
        {
            if (DataLoader.chartList.Find(item => item.sid == selectedSid) == null)
                selectedSid = DataLoader.chartList[Random.Range(0, DataLoader.chartList.Count)].sid;

            LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == selectedSid));
        }
        await SelectDefault();
        lg.enabled = false;
    }

    IEnumerator SelectDefault()
    {
        var background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        var path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        background.UpdateBackground(path);

        yield return new WaitForEndOfFrame();

        try
        {
            SelectSong(LiveSetting.currentChart);
        } 
        catch
        {

        }
    }

    IEnumerator SelectNear()
    {
        //yield return new WaitForSeconds(1);
        RectTransform[] rts = new RectTransform[SelectButtons.Count];
        for (int i = 0; i < SelectButtons.Count; i++)
        {
            rts[i] = SelectButtons[i].GetComponent<RectTransform>();
        }

        yield return 0;
        while (Mathf.Abs(rt_s.velocity.y) > scroll_Min_Speed || dh.isDragging)
        {
            yield return 0;
            //yield break;
        }
        //print("select near");
        rt_s.StopMovement();
        var destPos = 0 - rt.anchoredPosition.y - lg.padding.top - 100;
        float nearestDistance = 9999f;
        int nearstIndex = 0;
        for (int i = 0; i < SelectButtons.Count; i++)
        {
            float distance = Mathf.Abs(rts[i].anchoredPosition.y - destPos);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearstIndex = i;
            }

        }
        SelectSong(nearstIndex);
    }

    RectControl last = null;
    public void SelectSong(int index)
    {
        
        if (index == -1)
        {
            StartCoroutine(SelectNear());
            return;
        }

        last?.UnSelect();
        rcs[index].OnSelect();
        last = rcs[index];

        LiveSetting.currentChart = index;

        if (lastcHeader == LiveSetting.CurrentHeader) return;
        else
        {
            lastcHeader = LiveSetting.CurrentHeader; 
        }

        cl_lastsid.Set(LiveSetting.CurrentHeader.sid);
        difficultySelect.levels = LiveSetting.CurrentHeader.difficultyLevel.ToArray();
        difficultySelect.OnSongChange();
        PlayPreview();
    }

    public void UnselectSong()
    {
        //if (LiveSetting.currentChart >= 0)
        //{
        //    SelectButtons[LiveSetting.currentChart].GetComponent<RectControl>().UnSelect();
        //}
        last?.UnSelect();
    }

    bool isFirstPlay = true;
    private int lastPreviewMid = -1;

    async void PlayPreview()
    {
        await UniTask.WaitUntil(() => !faderWorking);

        if (LiveSetting.CurrentHeader.mid == lastPreviewMid)
            return;

        await PreviewFadeOut(0.02f);

        lastPreviewMid = LiveSetting.CurrentHeader.mid;

        mHeader mheader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);
        if (previewSound != null)
        {
            previewSound.Dispose();
            previewSound = null;
        }
        if (DataLoader.MusicExists(LiveSetting.CurrentHeader.mid))
        {
            previewSound = await AudioManager.Instance.PlayLoopMusic(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), true,
                new uint[]
                {
                    (uint)(mheader.preview[0] * 1000),
                    (uint)(mheader.preview[1] * 1000)
                },
                false
            );

            if (isFirstPlay)
            {
                previewSound?.Pause();
                await UniTask.Delay(2200);  //给语音留个地方
                previewSound?.Play();
            }

            await PreviewFadeIn();
            isFirstPlay = false;
        }
    }

    bool faderWorking = false;

    async UniTask PreviewFadeOut(float speed = 0.008F)
    {
        if (previewSound == null)
            return;

        faderWorking = true;

        for (float i = 0.7f; i > 0; i -= speed)
        {
            if (previewSound == null)
                return;

            previewSound.SetVolume(i);
            await UniTask.DelayFrame(0);
        }

        faderWorking = false;
    }

    async UniTask PreviewFadeIn()
    {
        if (previewSound == null)
            return;

        faderWorking = true;

        for (float i = 0f; i < 0.7f; i += 0.02f)
        {
            if (previewSound == null)
                return;

            previewSound.SetVolume(i);
            await UniTask.DelayFrame(0);
        }

        faderWorking = false;
    }

    public async void OnEnterPressed()
    {
        //if (!await LiveSetting.LoadChart(true))
        //{
        //    MainBlocker.Instance.SetBlock(false);
        //    return;
        //}
        //await PreviewFadeOut();
        SettingAndMod.instance.SetLiveSetting();

        KVSystem.Instance.SaveConfig();

        PlayVoicesAtSceneOut();
        SceneLoader.LoadScene("Select", "InGame", () => LiveSetting.LoadChart(true));
        await PreviewFadeOut();
    }

    #region ChartEditor
    public void OpenMappingScene()
    {
        //if (!await LiveSetting.LoadChart(false))
        //{
        //    return;
        //}
        SceneLoader.LoadScene("Select", "Mapping", () => LiveSetting.LoadChart(false));
    }

    public async void ExportKiraPack()
    {
        var prevOrientation = Screen.orientation;
        if (Application.platform == RuntimePlatform.Android)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            await UniTask.DelayFrame(0);
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
            await UniTask.DelayFrame(0);
            Screen.orientation = prevOrientation;
        }
    }

    public void DuplicateKiraPack()
    {
        DataLoader.DuplicateKiraPack(LiveSetting.CurrentHeader);
        SceneManager.LoadScene("Select");
    }
    #endregion

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
}

