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
using BanGround.Utils;
using BanGround.Scene.Params;
using BGEditor;
using BanGround.Game.Mods;

#pragma warning disable 0649
public class SelectManager_old : MonoBehaviour
{
    [Inject]
    private DiContainer _container;
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    public IDataLoader dataLoader;
    [Inject]
    private IKVSystem kvSystem;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IChartLoader chartLoader;
    [Inject]
    private IFileSystem fs;
    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;
    [Inject(Id = "cl_modflag")]
    private KVar cl_modflag;
    [Inject(Id = "g_saveReplay")]
    private KVar g_saveReplay;
    [Inject]
    private ICancellationTokenStore cancellationToken;

    public const float scroll_Min_Speed = 50f;

    //private RectTransform rt;
    //private ScrollRect rt_s;
    //private VerticalLayoutGroup lg;
    //[HideInInspector] public DragHandler dh;

    //sort
    private Text sort_Text;
    private Button sort_Button;

    //public GameObject songItemPrefab;
    [SerializeField] KiraScrollView scrollView = default;

    //private Transform songContent;

    [SerializeField] 
    private TextAsset[] voices;

    public List<cHeader> chartList => dataLoader.chartList;
    //private LinkedList<GameObject> SelectButtons = new LinkedList<GameObject>();
    //private LinkedList<RectTransform> rts = new LinkedList<RectTransform>();
    //private List<RectControl> rcs = new List<RectControl>();

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
        //rt = GameObject.Find("SongContent").GetComponent<RectTransform>();
        //rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        //dh = GameObject.Find("Song Scroll View").GetComponent<DragHandler>();
        //lg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();
        //songContent = GameObject.Find("SongContent").transform;
    }

    public void Return2Title()
    {
        SceneLoader.Back("Title");
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
        SettingAndMod.instance.SetLiveSetting();

        kvSystem.SaveConfig();

        PlayVoicesAtSceneOut();

        var modflag = ModFlagUtil.From(cl_modflag);
        SceneLoader.LoadScene("InGame", () => chartLoader.LoadChart(
            chartListManager.current.header.sid,
            chartListManager.current.difficulty,
            true), true,
            parameters: new InGameParams
            {
                sid = chartListManager.current.header.sid,
                difficulty = chartListManager.current.difficulty,
                isOffsetGuide = false,
                mods = modflag,
                saveRecord = true,
                saveReplay = g_saveReplay,
            });
        await PreviewFadeOut().WithCancellation(cancellationToken.sceneToken).SuppressCancellationThrow();
    }

    public void RefreshSongList()
    {
        var newIndices = new int[chartList.Count];

        for (int i = 0; i < chartList.Count; i++)
            newIndices[i] = i;

        scrollView.UpdateData(newIndices);
        scrollView.SelectCell(chartListManager.current.index);
        //lg.enabled = true;

        //// Adjust chartList
        //while (SelectButtons.Count < chartList.Count)
        //{
        //    var obj = _container.InstantiatePrefab(songItemPrefab, songContent);
        //    SelectButtons.AddLast(obj);
        //    rts.AddLast(obj.GetComponent<RectTransform>());
        //    var control = obj.GetComponent<RectControl>();
        //    control.index = rcs.Count;
        //    rcs.Add(control);
        //}
        //while (SelectButtons.Count > chartList.Count)
        //{
        //    Destroy(SelectButtons.Last.Value);
        //    SelectButtons.RemoveLast();
        //    rts.RemoveLast();
        //    rcs.RemoveAt(rcs.Count - 1);
        //}

        //// Spawn New SongItem
        //var curNode = SelectButtons.First;
        //for (int i = 0; i < chartList.Count; i++)
        //{
        //    var obj = curNode.Value;
        //    obj.name = i.ToString();
        //    Text[] txt = obj.GetComponentsInChildren<Text>();

        //    cHeader chart = chartList[i];
        //    mHeader song = dataLoader.GetMusicHeader(chart.mid);
        //    string author = chart.authorNick;
        //    txt[0].text = song.title;
        //    txt[1].text = song.artist + " / " + author;

        //    curNode = curNode.Next;
        //}

        //rt.sizeDelta = new Vector2(rt.sizeDelta.x, lg.padding.top * 2 + chartList.Count * (116) + (chartList.Count - 1) * lg.spacing + (800));

        //await SelectDefault();
        //lg.enabled = false;
    }

    //private async UniTask SelectDefault()
    //{
    //    var background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
    //    var path = dataLoader.GetBackgroundPath(chartListManager.current.header.sid).Item1;
    //    background.UpdateBackground(path);

    //    await UniTask.DelayFrame(1);

    //    try
    //    {
    //        SelectSong(chartListManager.current.index);
    //    } 
    //    catch
    //    {

    //    }
    //}

    //IEnumerator SelectNear()
    //{
    //    //yield return new WaitForSeconds(1);

    //    yield return 0;
    //    while (Mathf.Abs(rt_s.velocity.y) > scroll_Min_Speed || dh.isDragging)
    //    {
    //        yield return 0;
    //        //yield break;
    //    }
    //    rt_s.StopMovement();
    //    var destPos = 0 - rt.anchoredPosition.y - lg.padding.top - 100;
    //    float nearestDistance = 9999f;
    //    int nearstIndex = 0;
    //    int curIndex = 0;
    //    for (var node = rts.First; node != null; node = node.Next)
    //    {
    //        float distance = Mathf.Abs(node.Value.anchoredPosition.y - destPos);
    //        if (distance < nearestDistance)
    //        {
    //            nearestDistance = distance;
    //            nearstIndex = curIndex;
    //        }
    //        curIndex++;
    //    }
    //    SelectSong(nearstIndex);
    //}

    //private RectControl last = null;
    //private Coroutine selectCoroutine = null;
    //public void SelectSong(int index)
    //{
    //    if (index == -1)
    //    {
    //        if (selectCoroutine != null)
    //            StopCoroutine(selectCoroutine);
    //        selectCoroutine = StartCoroutine(SelectNear());
    //        return;
    //    }

    //    last?.UnSelect();
    //    rcs[index].OnSelect();
    //    last = rcs[index];

    //    chartListManager.SelectChartByIndex(index);

    //    PlayPreview().WithCancellation(cancellationToken.sceneToken).SuppressCancellationThrow().Forget();
    //}

    //public void UnselectSong()
    //{
    //    last?.UnSelect();
    //}

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
        ret[1] = preview.Length > 1 ? (uint)(preview[1] * 1000) : ret[0] + 10000;
        if (ret[0] > ret[1])
            (ret[0], ret[1]) = (ret[1], ret[0]);
        return ret;
    }

    async UniTask PlayPreview()
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
        int sid = chartListManager.current.header.sid;
        var difficulty = chartListManager.current.difficulty;
        SceneLoader.LoadScene("Mapping", () => chartLoader.LoadChart(sid, difficulty, false), true,
            new MappingParams
            {
                sid = sid,
                difficulty = difficulty,
                editor = new EditorInfo()
            });
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
        SceneLoader.LoadScene("Select");
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
            if (success) SceneLoader.LoadScene("Select");
        }
    }
#endif

    private void OnDestroy()
    {
        previewSound?.Stop();
        previewSound?.Dispose();
        SlideMesh.cacheMat = null;
        chartListManager.onChartListUpdated.RemoveListener(RefreshSongList);
    }
}

