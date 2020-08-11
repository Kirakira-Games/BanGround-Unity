﻿using System.Collections;
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
using Zenject;

#pragma warning disable 0649
public class SelectManager_old : MonoBehaviour
{
    [Inject]
    DiContainer _container;
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IKVSystem kvSystem;
    [Inject]
    private ILiveSetting liveSetting;
    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;
    [Inject(Id = "cl_lastsid")]
    private KVar cl_lastsid;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

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

    public List<cHeader> chartList => dataLoader.chartList;
    List<GameObject> SelectButtons = new List<GameObject>();
    RectTransform[] rts;
    List<RectControl> rcs = new List<RectControl>();

    DifficultySelect difficultySelect;

    public static SelectManager_old instance;

    [HideInInspector] 
    public ISoundTrack previewSound;

    static Kommand cmd_playdemo;
    
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

        dataLoader.LoadAllKiraPackFromInbox();
        dataLoader.RefreshSongList();

        InitComponent();
        InitSort();
        InitSongList(selectedSid);
        PlayVoicesAtSceneIn();
    }

    private async void PlayVoicesAtSceneIn()
    {
        (await audioManager.PrecacheSE(voices[Random.Range(0, 3)].bytes)).PlayOneShot();
    }

    private async void PlayVoicesAtSceneOut()
    {
        (await audioManager.PrecacheSE(voices[Random.Range(3, 7)].bytes)).PlayOneShot();
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
        InitSongList(liveSetting.CurrentHeader.sid);
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
                compare = new ChartDifSort(dataLoader, cl_lastdiff);
                break;
            case Sorter.SongName:
                compare = new SongNameSort(dataLoader);
                break;
            case Sorter.SongArtist:
                compare = new SongArtistSort(dataLoader);
                break;
            case Sorter.ChartAuthor:
                compare = new ChartAuthorSort();
                break;
            case Sorter.ChartScore:
                compare = new ChartScoreSort();
                break;
            default:
                compare = new SongNameSort(dataLoader);
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
            GameObject go = _container.InstantiatePrefab(songItemPrefab, pos);
            go.name = i.ToString();
            Text[] txt = go.GetComponentsInChildren<Text>();

            cHeader chart = chartList[i];
            mHeader song = dataLoader.GetMusicHeader(chart.mid);
            string author = chart.authorNick;
            txt[0].text = song.title;
            txt[1].text = song.artist + " / " + author;
            var rc = go.GetComponent<RectControl>();
            rc.index = i;
            rcs.Add(rc);
            SelectButtons.Add(go);
        }

        rts = new RectTransform[SelectButtons.Count];
        for (int i = 0; i < SelectButtons.Count; i++)
        {
            rts[i] = SelectButtons[i].GetComponent<RectTransform>();
        }

        //lg.padding = new RectOffset(0, 0, (int)((rt_v.sizeDelta.y / 2) - 100), 0);

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, lg.padding.top * 2 + chartList.Count * (116) + (chartList.Count - 1) * lg.spacing + (800));

        // After import, select imported chart
        if (dataLoader.LastImportedSid != -1)
        {
            selectedSid = dataLoader.LastImportedSid;
            dataLoader.LastImportedSid = -1;
        }
        if (selectedSid != -1)
        {
            if (dataLoader.chartList.Find(item => item.sid == selectedSid) == null)
                selectedSid = dataLoader.chartList[Random.Range(0, dataLoader.chartList.Count)].sid;

            liveSetting.currentChart = dataLoader.chartList.IndexOf(dataLoader.chartList.First(x => x.sid == selectedSid));
        }
        await SelectDefault();
        lg.enabled = false;
    }

    IEnumerator SelectDefault()
    {
        var background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        var path = dataLoader.GetBackgroundPath(liveSetting.CurrentHeader.sid).Item1;
        background.UpdateBackground(path);

        yield return new WaitForEndOfFrame();

        try
        {
            SelectSong(liveSetting.currentChart);
        } 
        catch
        {

        }
    }

    IEnumerator SelectNear()
    {
        //yield return new WaitForSeconds(1);

        yield return 0;
        while (Mathf.Abs(rt_s.velocity.y) > scroll_Min_Speed || dh.isDragging)
        {
            yield return 0;
            //yield break;
        }
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

    private RectControl last = null;
    private Coroutine selectCoroutine = null;
    public void SelectSong(int index)
    {
        if (index == -1)
        {
            if (selectCoroutine != null)
                StopCoroutine(selectCoroutine);
            selectCoroutine = StartCoroutine(SelectNear());
            return;
        }

        last?.UnSelect();
        rcs[index].OnSelect();
        last = rcs[index];

        liveSetting.currentChart = index;

        if (lastcHeader == liveSetting.CurrentHeader) return;
        else
        {
            lastcHeader = liveSetting.CurrentHeader; 
        }

        cl_lastsid.Set(liveSetting.CurrentHeader.sid);
        liveSetting.CurrentHeader.LoadDifficultyLevels(dataLoader);
        difficultySelect.levels = liveSetting.CurrentHeader.difficultyLevel.ToArray();
        difficultySelect.OnSongChange();
        PlayPreview();
    }

    public void UnselectSong()
    {
        //if (liveSetting.currentChart >= 0)
        //{
        //    SelectButtons[liveSetting.currentChart].GetComponent<RectControl>().UnSelect();
        //}
        last?.UnSelect();
    }

    bool isFirstPlay = true;
    private int lastPreviewMid = -1;

    private uint[] GetPreviewPos()
    {
        var ret = new uint[2];
        var preview = liveSetting.CurrentHeader.preview;
        if (preview == null || preview.Length == 0)
        {
            mHeader mheader = dataLoader.GetMusicHeader(liveSetting.CurrentHeader.mid);
            preview = mheader.preview;
        }
        ret[0] = (uint)(preview[0] * 1000);
        ret[1] = preview.Length > 1 ? (uint)(preview[1] * 1000) : ret[0];
        return ret;
    }

    async void PlayPreview()
    {
        await UniTask.WaitUntil(() => !faderWorking);

        if (liveSetting.CurrentHeader.mid == lastPreviewMid)
            return;

        await PreviewFadeOut(0.02f);

        lastPreviewMid = liveSetting.CurrentHeader.mid;

        if (previewSound != null)
        {
            previewSound.Dispose();
            previewSound = null;
        }
        if (dataLoader.MusicExists(liveSetting.CurrentHeader.mid))
        {
            previewSound = await audioManager.PlayLoopMusic(KiraFilesystem.Instance.Read(dataLoader.GetMusicPath(liveSetting.CurrentHeader.mid)), true,
                GetPreviewPos(),
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
        //if (!await liveSetting.LoadChart(true))
        //{
        //    MainBlocker.Instance.SetBlock(false);
        //    return;
        //}
        //await PreviewFadeOut();
        SettingAndMod.instance.SetLiveSetting();

        kvSystem.SaveConfig();

        PlayVoicesAtSceneOut();
        SceneLoader.LoadScene("Select", "InGame", () => liveSetting.LoadChart(true));
        await PreviewFadeOut();
    }

    #region ChartEditor
    public void OpenMappingScene()
    {
        //if (!await liveSetting.LoadChart(false))
        //{
        //    return;
        //}
        SceneLoader.LoadScene("Select", "Mapping", () => liveSetting.LoadChart(false));
    }

    public async void ExportKiraPack()
    {
        var prevOrientation = Screen.orientation;
        if (Application.platform == RuntimePlatform.Android)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            await UniTask.DelayFrame(0);
        }
        var zip = dataLoader.BuildKiraPack(liveSetting.CurrentHeader);
        var song = dataLoader.GetMusicHeader(liveSetting.CurrentHeader.mid);
        new NativeShare()
            .AddFile(zip)
            .SetSubject("Share " + song.title)
            .SetTitle("Share Kirapack")
            .SetText(song.title)
            .Share();
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", zip.Replace("/", "\\")));
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            await UniTask.DelayFrame(0);
            Screen.orientation = prevOrientation;
        }
    }

    public void DuplicateKiraPack()
    {
        dataLoader.DuplicateKiraPack(liveSetting.CurrentHeader);
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
            bool success = dataLoader.LoadAllKiraPackFromInbox();
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

