using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

public class SelectManager : MonoBehaviour
{
    public const float scroll_Min_Speed = 50f;
    //private Button enter_Btn;
    private Button setting_Open_Btn;
    private Button setting_Close_Btn;

    private int lastIndex = -1;

    private Toggle syncLine_Tog;
    private Toggle offBeat_Tog;
    private Toggle persp_Tog;
    private Toggle mirrow_Tog;

    private Toggle audioTrack_Tog;

    /* Mods     */

    // Auto
    private Toggle auto_Tog;
    // Double
    private Toggle double_Tog;
    // Half
    private Toggle half_Tog;

    /* Mods End */

    private InputField speed_Input;
    private InputField judge_Input;
    private InputField audio_Input;
    private InputField size_Input;

    private Slider bg_Bright;
    private Slider lane_Bright;
    private Slider long_Bright;
    private Slider seVolume_Input;
    private Slider bgmVolume_Input;

    private AudioManager audioManager;

    public GameObject enterAniObj;

    RectTransform rt ;
    RectTransform rt_v;
    ScrollRect rt_s;
    VerticalLayoutGroup lg ;
    DragHandler dh;

    RawImage Rank;
    RawImage clearMark;
    Text score;
    Text acc;

    private Animator scene_Animator;

    public GameObject songItemPrefab;

    public List<cHeader> chartList => DataLoader.chartList;
    List<GameObject> SelectButtons = new List<GameObject>();

    DifficultySelect difficultySelect;

    PlayRecords playRecords;

    // Start is called before the first frame update
    void Start()
    {
        InitComponent();
        LoadScoreRecord();
        InitSongList();
        GetLiveSetting();

        audioManager.loading = false;
    }

    private void InitComponent()
    {
        scene_Animator = GameObject.Find("SceneAnimator").GetComponent<Animator>();
        rt = GameObject.Find("SongContent").GetComponent<RectTransform>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        dh = GameObject.Find("Song Scroll View").GetComponent<DragHandler>();
        lg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();
        audioManager = AudioManager.Instanse;

        //enter_Btn = GameObject.Find("Enter_Btn").GetComponent<Button>();
        setting_Open_Btn = GameObject.Find("SettingOpenBtn").GetComponent<Button>();
        setting_Close_Btn = GameObject.Find("Button_Close").GetComponent<Button>();

        syncLine_Tog = GameObject.Find("Sync_Toggle").GetComponent<Toggle>();
        offBeat_Tog = GameObject.Find("Offbeat_Toggle").GetComponent<Toggle>();
        mirrow_Tog = GameObject.Find("Mirrow_Toggle").GetComponent<Toggle>();
        persp_Tog = GameObject.Find("Perspective_Toggle").GetComponent<Toggle>();

        speed_Input = GameObject.Find("Speed_Input").GetComponent<InputField>();
        judge_Input = GameObject.Find("Judge_Input").GetComponent<InputField>();
        audio_Input = GameObject.Find("Audio_Input").GetComponent<InputField>();
        size_Input = GameObject.Find("Size_Input").GetComponent<InputField>();

        bg_Bright = GameObject.Find("BG_Bri_Slider").GetComponent<Slider>();
        lane_Bright = GameObject.Find("Lane_Bri_Slider").GetComponent<Slider>();
        long_Bright = GameObject.Find("Long_Bri_Slider").GetComponent<Slider>();
        seVolume_Input = GameObject.Find("SeVolume_Input").GetComponent<Slider>();
        bgmVolume_Input = GameObject.Find("BGMVolume_Input").GetComponent<Slider>();

        auto_Tog = GameObject.Find("Autoplay_Toggle").GetComponent<Toggle>();
        half_Tog = GameObject.Find("Half_Toggle").GetComponent<Toggle>();
        double_Tog = GameObject.Find("Double_Toggle").GetComponent<Toggle>();

        audioTrack_Tog = GameObject.Find("AudioTrack_Toggle").GetComponent<Toggle>();

        //enter_Btn.onClick.AddListener(OnEnterPressed);
        setting_Open_Btn.onClick.AddListener(OpenSetting);
        setting_Close_Btn.onClick.AddListener(CloseSetting);

        GameObject.Find("Speed>").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) + 0.1f).ToString(); });
        GameObject.Find("Speed<").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) - 0.1f).ToString(); });
        GameObject.Find("Speed>>").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) + 1f).ToString(); });
        GameObject.Find("Speed<<").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) - 1f).ToString(); });
        speed_Input.onValueChanged.AddListener((string a) =>
        {
            if (float.Parse(speed_Input.text) < 1) { speed_Input.text = "11"; }
            if (float.Parse(speed_Input.text) > 11f) { speed_Input.text = "1"; }
            speed_Input.text = string.Format("{0:F1}", float.Parse(speed_Input.text));
        });

        GameObject.Find("Size>").GetComponent<Button>().onClick.AddListener(() => { size_Input.text = (float.Parse(size_Input.text) + 0.1f).ToString(); });
        GameObject.Find("Size<").GetComponent<Button>().onClick.AddListener(() => { size_Input.text = (float.Parse(size_Input.text) - 0.1f).ToString(); });
        size_Input.onValueChanged.AddListener((string a) =>
        {
            if (float.Parse(size_Input.text) < 0.1f) { size_Input.text = "2"; }
            if (float.Parse(size_Input.text) > 2f) { size_Input.text = "0.1"; }
            size_Input.text = string.Format("{0:F1}", float.Parse(size_Input.text));
        });

        GameObject.Find("JudOff>").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) + 1f).ToString(); });
        GameObject.Find("JudOff<").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) - 1f).ToString(); });

        GameObject.Find("AudOff>").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) + 1f).ToString(); });
        GameObject.Find("AudOff<").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) - 1f).ToString(); });
        //live setting init

        Rank = GameObject.Find("Rank").GetComponent<RawImage>();
        clearMark = GameObject.Find("ClearMark").GetComponent<RawImage>();
        score = GameObject.Find("ScoreHistory").GetComponent<Text>();
        acc = GameObject.Find("AccText").GetComponent<Text>();

        difficultySelect = GameObject.Find("DifficultySelect").GetComponent<DifficultySelect>();
    }
    void LoadScoreRecord()
    {
        playRecords = PlayRecords.OpenRecord();
    }
    

    //--------------------------------------------
    private void InitSongList()
    {
        for (int i = 0; i < chartList.Count; i++)
        {
            GameObject go = Instantiate(songItemPrefab, GameObject.Find("SongContent").transform);
            go.name = i.ToString();
            Text[] txt = go.GetComponentsInChildren<Text>();

            cHeader chart = chartList[i];
            mHeader song = DataLoader.GetMusicHeader(chart.mid);
            string author = chart.author;
            txt[0].text = song.title;
            txt[1].text = song.artist + " / " + author;
            go.GetComponent<RectControl>().index = i;
            SelectButtons.Add(go);
        }

        lg.padding = new RectOffset(0, 0, (int)((rt_v.sizeDelta.y / 2) - 100),0);

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, lg.padding.top * 2 + chartList.Count * (116) + (chartList.Count - 1) * lg.spacing + (200 - 116));
        StartCoroutine(SelectDefault());
    }

    IEnumerator SelectDefault()
    {
        var background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        var path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid);
        background.UpdateBackground(path);

        yield return new WaitForEndOfFrame();
        try
        {
            SelectSong(LiveSetting.currentChart);
        }catch
        {

        }
    }

    IEnumerator SelectNear()
    {
        //yield return new WaitForSeconds(1);
        RectTransform[] rts = new RectTransform[SelectButtons.Count];
        for(int i =0;i<SelectButtons.Count;i++)
        {
            rts[i] = SelectButtons[i].GetComponent<RectTransform>();
        }

        yield return 0;
        while (Mathf.Abs(rt_s.velocity.y) > scroll_Min_Speed || dh.isDragging)
        {
            yield return 0;
        }
        print("select near");
        rt_s.StopMovement();
        var destPos = 0 - rt.anchoredPosition.y - lg.padding.top - 100;
        float nearestDistance = 9999f;
        int nearstIndex = 0;
        for (int i = 0; i < SelectButtons.Count; i++)
        {
            float distance =  Mathf.Abs( rts[i].anchoredPosition.y - destPos);
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
        if (lastIndex == LiveSetting.currentChart) return;
        else lastIndex = LiveSetting.currentChart;

        difficultySelect.levels = LiveSetting.CurrentHeader.difficultyLevel.ToArray();
        difficultySelect.OnSongChange();
        //DisplayRecord();
        PlayPreview();
    }

    public void UnselectSong()
    {
        if (LiveSetting.currentChart >= 0)
        {
            SelectButtons[LiveSetting.currentChart].GetComponent<RectControl>().UnSelect();
        }
    }

    public void DisplayRecord()
    {
        int count = 0;
        PlayResult a = new PlayResult();
        for (int i = 0; i < playRecords.resultsList.Count; i++)
        {
            if (playRecords.resultsList[i].ChartId == LiveSetting.CurrentHeader.sid &&
                playRecords.resultsList[i].Difficulty == (Difficulty)LiveSetting.actualDifficulty)
            {
                count++;
                a = playRecords.resultsList[i];
                
            }
        }
        score.text = string.Format("{0:0000000}", a.Score);
        acc.text = string.Format("{0:P2}", a.Acc);
        //Set Rank
        if (count == 0)
        {
            Rank.enabled = false;
            clearMark.enabled = false;
            return;
        }
        else
        {
            Rank.enabled = true;
            clearMark.enabled = true;
        }
        
            
        var rank = new Texture2D(0, 0);
        switch (a.ranks)
        {
            case Ranks.SSS:
                rank = Resources.Load(LiveSetting.IconPath + "SSS") as Texture2D;
                break;
            case Ranks.SS:
                rank = Resources.Load(LiveSetting.IconPath + "SS") as Texture2D;
                break;
            case Ranks.S:
                rank = Resources.Load(LiveSetting.IconPath + "S") as Texture2D;
                break;
            case Ranks.A:
                rank = Resources.Load(LiveSetting.IconPath + "A") as Texture2D;
                break;
            case Ranks.B:
                rank = Resources.Load(LiveSetting.IconPath + "B") as Texture2D;
                break;
            case Ranks.C:
                rank = Resources.Load(LiveSetting.IconPath + "C") as Texture2D;
                break;
            case Ranks.D:
                rank = Resources.Load(LiveSetting.IconPath + "D") as Texture2D;
                break;
            case Ranks.F:
                rank = Resources.Load(LiveSetting.IconPath + "F") as Texture2D;
                break;
            default:
                rank = null;
                break;
        }
        Rank.texture = rank;

        //Set Mark
        var mark = new Texture2D(0, 0);
        switch (a.clearMark)
        {
            case ClearMarks.AP:
                mark = Resources.Load(LiveSetting.IconPath + "AP") as Texture2D;
                break;
            case ClearMarks.FC:
                mark = Resources.Load(LiveSetting.IconPath + "FC") as Texture2D;
                break;
            case ClearMarks.CL:
                mark = Resources.Load(LiveSetting.IconPath + "CL") as Texture2D;
                break;
            case ClearMarks.F:
                clearMark.enabled = false;
                break;
            default:
                clearMark.enabled = false;
                break;
        }
        clearMark.texture = mark;
    }

    LoopingBassMemStream lastPreviewStream = null;

    void PlayPreview()
    {
        //if (lastIndex == LiveSetting.selectedIndex) return;
        //else lastIndex = LiveSetting.selectedIndex;

        LoopingBassMemStream.DisposeAll();
        mHeader mheader = DataLoader.GetMusicHeader(LiveSetting.CurrentHeader.mid);

        lastPreviewStream = audioManager.StreamLoopSound(File.ReadAllBytes(
            DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)),
            mheader.preview[0],
            mheader.preview[1]);

        lastPreviewStream.Play();
    }

    //--------------------------------------------
    void OpenSetting()
    {
        GetModStatus();
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetBool("Drop", true);
    }
    void CloseSetting()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetBool("Drop", false);
        SetLiveSetting();
        File.WriteAllText(LiveSetting.settingsPath, JsonConvert.SerializeObject(new LiveSettingTemplate()));
    }
    void GetLiveSetting()
    {
        speed_Input.text = LiveSetting.noteSpeed.ToString();
        judge_Input.text = LiveSetting.judgeOffset.ToString();
        audio_Input.text = LiveSetting.audioOffset.ToString();
        size_Input.text = LiveSetting.noteSize.ToString();
        syncLine_Tog.isOn = LiveSetting.syncLineEnabled;
        offBeat_Tog.isOn = LiveSetting.grayNoteEnabled;
        mirrow_Tog.isOn = LiveSetting.mirrowEnabled;
        auto_Tog.isOn = LiveSetting.autoPlayEnabled;
        persp_Tog.isOn = LiveSetting.bangPerspective;

        bg_Bright.value = LiveSetting.bgBrightness;
        lane_Bright.value = LiveSetting.laneBrightness;
        long_Bright.value = LiveSetting.longBrightness;

        seVolume_Input.value = LiveSetting.seVolume;
        bgmVolume_Input.value = LiveSetting.bgmVolume;

        audioTrack_Tog.isOn = LiveSetting.enableAudioTrack;
    }
    void GetModStatus()
    {
        half_Tog.isOn = LiveSetting.attachedMods.Contains(HalfMod.Instanse);
        double_Tog.isOn = LiveSetting.attachedMods.Contains(DoubleMod.Instanse);
    }

    public void OnLanuageChanged(int value)
    {
        switch (value)
        {
            case 0:
                LiveSetting.language = Language.English;
                break;
            case 1:
                LiveSetting.language = Language.SimplifiedChinese;
                break;
            case 2:
                LiveSetting.language = Language.TraditionalChinese;
                break;
            case 3:
                LiveSetting.language = Language.Japanese;
                break;
            case 4:
                LiveSetting.language = Language.Korean;
                break;
            default:
                Debug.LogError("爪巴~");
                break;
        }

        LocalizedStrings.Instanse.ReloadLanguageFile(LiveSetting.language);
        LocalizedText.ReloadAll();
    }

    void SetLiveSetting()
    {
        LiveSetting.noteSpeed = float.Parse(speed_Input.text);
        LiveSetting.judgeOffset = int.Parse(judge_Input.text);
        LiveSetting.audioOffset = int.Parse(audio_Input.text);
        LiveSetting.noteSize = float.Parse(size_Input.text);
        LiveSetting.seVolume = seVolume_Input.value;
        LiveSetting.bgmVolume = bgmVolume_Input.value;
        LiveSetting.syncLineEnabled = syncLine_Tog.isOn;
        LiveSetting.grayNoteEnabled = offBeat_Tog.isOn;
        LiveSetting.mirrowEnabled = mirrow_Tog.isOn;
        LiveSetting.autoPlayEnabled = auto_Tog.isOn;
        LiveSetting.bangPerspective = persp_Tog.isOn;

        LiveSetting.bgBrightness = bg_Bright.value;
        LiveSetting.laneBrightness = lane_Bright.value;
        LiveSetting.longBrightness = long_Bright.value;

        LiveSetting.enableAudioTrack = audioTrack_Tog.isOn;

        if(!double_Tog.isOn)
            LiveSetting.RemoveMod(DoubleMod.Instanse);

        if(!half_Tog.isOn)
            LiveSetting.RemoveMod(HalfMod.Instanse);

        if (double_Tog.isOn)
            LiveSetting.AddMod(DoubleMod.Instanse);

        if (half_Tog.isOn)
            LiveSetting.AddMod(HalfMod.Instanse);
    }

    public void OnDoubleModChange()
    {
        if (double_Tog.isOn)
            half_Tog.isOn = false;
    }

    public void OnHalfModChange()
    {
        if (half_Tog.isOn)
            double_Tog.isOn = false;
    }
    //============================================
    public void OnEnterPressed()
    {
        /*if (!isSettingOpened)
        {
            OpenSetting();
            return;
        }
        */
        /*
        var toggles = selectGroup.ActiveToggles();
        foreach (var seleted in toggles)
        {
            //Debug.Log(seleted.name);
            LiveSetting.selected = seleted.name;
        }
        */
        //enter_Btn.interactable = false;
        //enterAniObj.SetActive(true);
        //scene_Animator.Play("OutPlay", -1, 0);
        CloseSetting();
        setting_Open_Btn.gameObject.SetActive(false);


        StartCoroutine(DelayLoadScene());

    }
    IEnumerator DelayLoadScene()
    {
        float delay = 1.2f;

        float startVolume = lastPreviewStream.Volume;

        SceneLoader.LoadScene("Select", "InGame");

        while (delay >= 0)
        {
            yield return new WaitForEndOfFrame();
            delay -= Time.deltaTime;
            lastPreviewStream.Volume = startVolume * (delay / 1.2f);
        }

        lastPreviewStream.Dispose();

        //yield return new WaitForSeconds(2f);
        //SceneManager.LoadSceneAsync("InGame");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            lastPreviewStream?.Pause();
        else
            lastPreviewStream?.Play();
    }
}

