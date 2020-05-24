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
using Random = UnityEngine.Random;

#pragma warning disable 0649
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

    [SerializeField] 
    private TextAsset[] voices;

    public List<cHeader> chartList => DataLoader.chartList;
    List<GameObject> SelectButtons = new List<GameObject>();

    DifficultySelect difficultySelect;

    public static SelectManager instance;

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

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;


        int selectedSid = cl_lastsid;

        DataLoader.LoadAllKiraPackFromInbox();
        DataLoader.RefreshSongList();
        DataLoader.ReloadSongList();

        InitComponent();
        InitSort();
        InitSongList(selectedSid);
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
    public void InitSongList(int selectedSid = -1)
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

    public IEnumerator SelectNear()
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
        else
        {
            lastcHeader = LiveSetting.CurrentHeader; 
            StartCoroutine(PreviewFadeOut(0.02f));
        }

        cl_lastsid.Set(LiveSetting.CurrentHeader.sid);
        difficultySelect.levels = LiveSetting.CurrentHeader.difficultyLevel.ToArray();
        difficultySelect.OnSongChange();
        StopCoroutine(PlayPreview());
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
            StopCoroutine("PreviewFadeIn");
            StartCoroutine(PreviewFadeIn());
            isFirstPlay = false;
        }
    }

    IEnumerator PreviewFadeOut(float speed =0.008F)
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

    public void OnEnterPressed()
    {
        StartCoroutine(PreviewFadeOut());
        SettingAndMod.instance.SetLiveSetting();

        KVSystem.Instance.SaveConfig();

        PlayVoicesAtSceneOut();
        SceneLoader.LoadScene("Select", "InGame", true);
    }

    public void OpenMappingScene()
    {
        SceneLoader.LoadScene("Select", "Mapping", true);
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

