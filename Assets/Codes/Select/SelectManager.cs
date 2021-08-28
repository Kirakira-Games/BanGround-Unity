using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using AudioProvider;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround;
using BanGround.Scene.Params;
using BGEditor;
using BanGround.Game.Mods;
using BanGround.Identity;

#pragma warning disable 0649
public class SelectManager : MonoBehaviour
{
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
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private IMessageBox messageBox;
    [Inject]
    private SettingPanel settingPanel;
    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;
    [Inject(Id = "cl_modflag")]
    private KVar cl_modflag;
    [Inject(Id = "g_saveReplay")]
    private KVar g_saveReplay;
    [Inject]
    private ICancellationTokenStore cancellationToken;
    [Inject]
    private IAccountManager accountManager;

    public int CurrentPlayingSid { get; private set; }
    private float bgmVolume = 0.0f;
    public bool BGMIsMuted => bgmVolume == 0.0f;

    public const float scroll_Min_Speed = 50f;

    //private RectTransform rt;
    //private ScrollRect rt_s;
    //private VerticalLayoutGroup lg;
    //[HideInInspector] public DragHandler dh;

    //sort
    private Text sort_Text;
    private Button sort_Button;

    [SerializeField] private Button delete_Button;

    //public GameObject songItemPrefab;
    [SerializeField] KiraScrollView scrollView = default;

    //private Transform songContent;

    [SerializeField] 
    private TextAsset[] voices;

    public List<V2.cHeader> chartList => dataLoader.chartList;

    [HideInInspector] 
    public ISoundTrack previewSound;

    async void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        InitComponent();
        GameObject.Find("UserInfo").GetComponent<UserInfo>().GetUserInfo().Forget();

        // Register callback
        chartListManager.onChartListUpdated.AddListener(RefreshSongList);

        await dataLoader.LoadAllKiraPackFromInbox();
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

        delete_Button.onClick.AddListener(async () =>
        {
            if (await messageBox.ShowMessage("Select.Title.Delete".L(), "Select.Prompt.Delete".L()))
            {
                OnDelete();
            }
        });
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
        settingPanel.SetLiveSetting();

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
    }

    public void SelectSong(int index)
    {
        chartListManager.SelectChartByIndex(index);

        PlayPreview().WithCancellation(cancellationToken.sceneToken).SuppressCancellationThrow().Forget();
    }

    public void SelectRandom()
    {
        int N = chartListManager.chartList.Count;
        if (N <= 1)
            return;
        int index = Random.Range(0, N);
        scrollView.SelectCell(index);
    }

    private bool isFirstPlay = true;
    private int lastPreviewMid = -1;

    private uint[] GetPreviewPos()
    {
        var ret = new uint[2];
        var preview = chartListManager.current.header.preview;
        if (preview == null || preview.Length == 0)
        {
            var mheader = dataLoader.GetMusicHeader(chartListManager.current.header.mid);
            preview = mheader.preview;
        }
        ret[0] = (uint)(preview[0] * 1000);
        ret[1] = preview.Length > 1 ? (uint)(preview[1] * 1000) : ret[0] + 10000;
        if (ret[0] > ret[1])
            (ret[0], ret[1]) = (ret[1], ret[0]);
        return ret;
    }

    bool isWaiting = false;

    async UniTask PlayPreview()
    {
        await UniTask.WaitUntil(() => !faderWorking);

        if (isWaiting)
            return;

        if(isFirstPlay)
        {
            isWaiting = true;

            //给语音留个地方
            await UniTask.Delay(2200);

            isWaiting = false;
            isFirstPlay = false;
        }

        if (this == null)
            return;

        if (chartListManager.current.header.mid == lastPreviewMid)
        {
            CurrentPlayingSid = chartListManager.current.header.sid;
            return;
        }

        lastPreviewMid = chartListManager.current.header.mid;

        if (previewSound != null)
        {
            previewSound.Dispose();
            previewSound = null;
        }
        if (dataLoader.MusicExists(lastPreviewMid))
        {
            await UniTask.SwitchToMainThread();

            previewSound = await audioManager.PlayLoopMusic(fs.GetFile(dataLoader.GetMusicPath(lastPreviewMid)).ReadToEnd(), true,
                GetPreviewPos(),
                false
            );

            CurrentPlayingSid = chartListManager.current.header.sid;
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

        for (float i = 0f; i < 0.7f; i += 0.005f)
        {
            if (previewSound == null)
                return;

            previewSound.SetVolume(i);
            await UniTask.DelayFrame(1);
        }

        faderWorking = false;
    }

    public void SetPreviewVolume(float volume)
    {
        //Debug.Log(volume);
        previewSound?.SetVolume(volume * 0.7f);
    }

    private void OnDelete()
    {
        var header = chartListManager.current.header;
        V2.Difficulty difficulty = chartListManager.current.difficulty;
        if (header.sid == OffsetGuide.OFFSET_GUIDE_SID)
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Select.DeleteOffsetGuideTip".L());
            return;
        }

        bool hasOtherDifficulty = false;
        for (int i = 0; i < header.difficultyLevel.Count; i++)
        {
            if (i == (int)difficulty)
                continue;
            if (header.difficultyLevel[i] != -1)
            {
                hasOtherDifficulty = true;
                break;
            }
        }
        if (hasOtherDifficulty)
        {
            dataLoader.DeleteDifficulty(header.sid, difficulty);
        }
        else
        {
            dataLoader.DeleteChart(header.sid);
        }

        SceneLoader.LoadSceneAsync("Select");
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
    private async void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            bool success = await dataLoader.LoadAllKiraPackFromInbox();
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

    public async void OpenCommunityScene()
    {
        //必须要在线状态才能进社区
        if (accountManager.isOfflineMode && !await accountManager.DoLogin())
        {
            return;
        }
        SceneLoader.LoadScene("Community", pushStack: true);
    }
}

