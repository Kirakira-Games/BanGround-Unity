using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System;
using AudioProvider;

public class SelectManager : MonoBehaviour
{
    public const float scroll_Min_Speed = 50f;

    private cHeader lastcHeader = new cHeader();

    RectTransform rt;
    RectTransform rt_v;
    ScrollRect rt_s;
    VerticalLayoutGroup lg;
    DragHandler dh;

    //sort
    private Text sort_Text;
    private Button sort_Button;

    public GameObject songItemPrefab;

    [SerializeField] private TextAsset[] voices;

    public List<cHeader> chartList => DataLoader.chartList;
    List<GameObject> SelectButtons = new List<GameObject>();

    DifficultySelect difficultySelect;

    public static SelectManager instance;

    [HideInInspector] public ISoundTrack previewSound;

    private void Awake()
    {
        instance = this;
        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        InitComponent();
        InitSort();
        InitSongList(false);
        PlayVoicesAtSceneIn();
    }

    private void PlayVoicesAtSceneIn()
    {
        AudioManager.Instance.PrecacheSE(voices[UnityEngine.Random.Range(0,3)].bytes).PlayOneShot();
    }

    private void PlayVoicesAtSceneOut()
    {
        AudioManager.Instance.PrecacheSE(voices[UnityEngine.Random.Range(3, 7)].bytes).PlayOneShot();
    }

    private void InitComponent()
    {
        //sort
        sort_Button = GameObject.Find("Sort_Button").GetComponent<Button>();
        sort_Text = GameObject.Find("Sort_Text").GetComponent<Text>();
        sort_Button.onClick.AddListener(SwitchSort);

        //Main Scroll View
        rt = GameObject.Find("SongContent").GetComponent<RectTransform>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        dh = GameObject.Find("Song Scroll View").GetComponent<DragHandler>();
        lg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();

        difficultySelect = GameObject.Find("DifficultySelect").GetComponent<DifficultySelect>();
    }

    //---------------------------------------------
    void SwitchSort()
    {
        LiveSetting.sort++;
        if ((int)LiveSetting.sort > 4) LiveSetting.sort = 0;
        sort_Text.text = Enum.GetName(typeof(Sorter), LiveSetting.sort);
        InitSongList();
    }

    void InitSort()
    {
        sort_Text.text = Enum.GetName(typeof(Sorter), LiveSetting.sort);
    }

    //Song Selection-------------------------------
    public void InitSongList(bool saveSid = true)
    {
        //Save Sid
        int sid = DataLoader.chartList[LiveSetting.currentChart].sid;

        // Sort SongList
        IComparer<cHeader> compare;
        switch (LiveSetting.sort)
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

        //Spawn New SongItem
        for (int i = 0; i < chartList.Count; i++)
        {
            GameObject go = Instantiate(songItemPrefab, GameObject.Find("SongContent").transform);
            go.name = i.ToString();
            Text[] txt = go.GetComponentsInChildren<Text>();

            cHeader chart = chartList[i];
            mHeader song = DataLoader.GetMusicHeader(chart.mid);
            string author = chart.authorNick;
            txt[0].text = song.title;
            txt[1].text = song.artist + " / " + author;
            go.GetComponent<RectControl>().index = i;
            SelectButtons.Add(go);
        }

        lg.padding = new RectOffset(0, 0, (int)((rt_v.sizeDelta.y / 2) - 100), 0);

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, lg.padding.top * 2 + chartList.Count * (116) + (chartList.Count - 1) * lg.spacing + (200 - 116));

        // After import, select imported chart
        if (DataLoader.LastImportedSid != -1)
        {
            sid = DataLoader.LastImportedSid;
            saveSid = true;
            DataLoader.LastImportedSid = -1;
        }
        if (saveSid)
        {
            LiveSetting.currentChart = DataLoader.chartList.IndexOf(DataLoader.chartList.First(x => x.sid == sid));
        }

        StartCoroutine(SelectDefault());
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

    public void SelectSong(int index)
    {
        if (index == -1)
        {
            StartCoroutine(SelectNear());
            return;
        }

        foreach (GameObject selected in SelectButtons)
        {

            RectControl rc = selected.GetComponent<RectControl>();
            rc.StopAllCoroutines();
            if (rc.index != index)
                rc.UnSelect();
            else
                rc.OnSelect();
        }

        LiveSetting.currentChart = index;
        if (lastcHeader == LiveSetting.CurrentHeader) return;
        else lastcHeader = LiveSetting.CurrentHeader;

        difficultySelect.levels = LiveSetting.CurrentHeader.difficultyLevel.ToArray();
        difficultySelect.OnSongChange();
        //DisplayRecord();
        StartCoroutine(PlayPreview());
    }

    public void UnselectSong()
    {
        if (LiveSetting.currentChart >= 0)
        {
            SelectButtons[LiveSetting.currentChart].GetComponent<RectControl>().UnSelect();
        }
    }

    

    bool isFirstPlay = true;
    IEnumerator PlayPreview()
    {
        mHeader mheader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);

        if (previewSound != null) {
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
            isFirstPlay = false;
        }
    }

    IEnumerator PreviewFadeOut()
    {
        for (float i = 1f; i > 0; i -= 0.2f)
        {
            previewSound.SetVolume(i);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnEnterPressed()
    {
        StartCoroutine(PreviewFadeOut());
        SettingAndMod.instance.SetLiveSetting();
        File.WriteAllText(LiveSetting.settingsPath, JsonConvert.SerializeObject(new LiveSettingTemplate()));
        PlayVoicesAtSceneOut();
        SceneLoader.LoadScene("Select", "InGame", true);
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
}

