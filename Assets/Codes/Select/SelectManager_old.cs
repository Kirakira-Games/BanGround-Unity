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
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround;

#pragma warning disable 0649
public class SelectManager_old : MonoBehaviour
{
    [Inject]
    private DiContainer _container;
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IKVSystem kvSystem;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IFileSystem fs;
    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;

    public const float scroll_Min_Speed = 50f;

    private RectTransform rt;
    private ScrollRect rt_s;
    private VerticalLayoutGroup lg;
    public DragHandler dh;

    //sort
    private Text sort_Text;
    private Button sort_Button;

    public GameObject songItemPrefab;

    [SerializeField] 
    private TextAsset[] voices;

    public List<cHeader> chartList => dataLoader.chartList;
    private List<GameObject> SelectButtons = new List<GameObject>();
    private RectTransform[] rts;
    private List<RectControl> rcs = new List<RectControl>();

    public static SelectManager_old instance;

    [HideInInspector] 
    public ISoundTrack previewSound;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        InitComponent();
        GameObject.Find("UserInfo").GetComponent<UserInfo>().GetUserInfo().Forget();

        // Register callback
        chartListManager.onChartListUpdated.AddListener(RefreshSongList);

        dataLoader.LoadAllKiraPackFromInbox();
        dataLoader.RefreshSongList();

        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
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
    }

    #region Controller
    void SwitchSort()
    {
        cl_cursorter.Set((cl_cursorter + 1) % 5);
        sort_Text.text = Enum.GetName(typeof(Sorter), (Sorter)cl_cursorter);
        chartListManager.SortChart();
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
        SceneLoader.LoadScene("Select", "InGame", () => chartListManager.LoadChart(true));
        await PreviewFadeOut();
    }

    public async void RefreshSongList()
    {
        lg.enabled = true;

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

        await SelectDefault();
        lg.enabled = false;
    }

    private async UniTask SelectDefault()
    {
        var background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        var path = dataLoader.GetBackgroundPath(chartListManager.current.header.sid).Item1;
        background.UpdateBackground(path);

        await UniTask.DelayFrame(1);

        try
        {
            SelectSong(chartListManager.current.index);
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

        chartListManager.SelectChartByIndex(index);

        PlayPreview();
    }

    public void UnselectSong()
    {
        last?.UnSelect();
    }

    private bool isFirstPlay = true;
    private int lastPreviewMid = -1;

    private uint[] GetPreviewPos()
    {
        var ret = new uint[2];
        var preview = chartListManager.current.header.preview;
        if (preview == null || preview.Length == 0)
        {
            mHeader mheader = dataLoader.GetMusicHeader(chartListManager.current.header.mid);
            preview = mheader.preview;
        }
        ret[0] = (uint)(preview[0] * 1000);
        ret[1] = preview.Length > 1 ? (uint)(preview[1] * 1000) : ret[0];
        return ret;
    }

    async void PlayPreview()
    {
        await UniTask.WaitUntil(() => !faderWorking);

        if (chartListManager.current.header.mid == lastPreviewMid)
            return;

        await PreviewFadeOut(0.02f);

        lastPreviewMid = chartListManager.current.header.mid;

        if (previewSound != null)
        {
            previewSound.Dispose();
            previewSound = null;
        }
        if (dataLoader.MusicExists(lastPreviewMid))
        {
            previewSound = await audioManager.PlayLoopMusic(fs.GetFile(dataLoader.GetMusicPath(lastPreviewMid)).ReadToEnd(), true,
                GetPreviewPos(),
                false
            );

            if (isFirstPlay)
            {
                previewSound?.Pause();
                await UniTask.Delay(2200);  //给语音留个地方
                previewSound?.Play();
                isFirstPlay = false;
            }

            await PreviewFadeIn();
        }
    }

    private bool faderWorking = false;

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
            await UniTask.DelayFrame(1);
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
            await UniTask.DelayFrame(1);
        }

        faderWorking = false;
    }
    #endregion

    #region ChartEditor
    public void OpenMappingScene()
    {
        //if (!await liveSetting.LoadChart(false))
        //{
        //    return;
        //}
        SceneLoader.LoadScene("Select", "Mapping", () => chartListManager.LoadChart(false));
    }

    public async void ExportKiraPack()
    {
        var prevOrientation = Screen.orientation;
        if (Application.platform == RuntimePlatform.Android)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            await UniTask.DelayFrame(1);
        }
        var zip = dataLoader.BuildKiraPack(chartListManager.current.header);
        var song = dataLoader.GetMusicHeader(chartListManager.current.header.mid);
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
            await UniTask.DelayFrame(1);
            Screen.orientation = prevOrientation;
        }
    }

    public void DuplicateKiraPack()
    {
        dataLoader.DuplicateKiraPack(chartListManager.current.header);
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
        chartListManager.onChartListUpdated.RemoveListener(RefreshSongList);
    }
}

