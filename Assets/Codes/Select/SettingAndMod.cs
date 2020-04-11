using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioProvider;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;

public class SettingAndMod : MonoBehaviour
{
    public static SettingAndMod instance;

    private Button setting_Open_Btn;
    private Button setting_Close_Btn;
    private Button mod_Open_Btn;
    private Button mod_Close_Btn;

    private Toggle syncLine_Tog;
    private Toggle offBeat_Tog;
    private Toggle persp_Tog;
    private Toggle mirrow_Tog;
    private Slider ELP_Slider;
    private Toggle FS_Tog;
    private Toggle VSync_Tog;
    private Toggle laneLight_Tog;
    private Toggle shake_Tog;
    private Toggle milisec_Tog;
    private Toggle Video_Tog;
    private NoteStyleToggleGroup noteToggles;
    private SESelector seSelector;

    /* Mods     */

    // Auto
    private Toggle auto_Tog;
    // Double
    private StepToggle speedUp_Tog;
    // Half
    private StepToggle speedDown_Tog;
    //Sudden Death
    private Toggle suddenDeath_Tog;
    //Perfect
    private Toggle perfect_Tog;

    /* Mods End */

    private InputField speed_Input;
    private InputField judge_Input;
    private InputField audio_Input;
    private InputField size_Input;

    private Slider judgeOffsetTransform;
    private Slider far_Clip;
    private Slider bg_Bright;
    private Slider lane_Bright;
    private Slider long_Bright;
    private Slider seVolume_Input;
    private Slider igseVolume_Input;
    private Slider bgmVolume_Input;

    private Dropdown language_Dropdown;
    
    void initCompoinents()
    {
#if !(UNITY_STANDALONE || UNITY_WSA)
        GameObject.Find("Fullscreen").SetActive(false);
        GameObject.Find("Fullscreen_Toggle").SetActive(false);
        GameObject.Find("VSync").SetActive(false);
        GameObject.Find("VSync_Toggle").SetActive(false);
#else
        FS_Tog = GameObject.Find("Fullscreen_Toggle").GetComponent<Toggle>();
        VSync_Tog = GameObject.Find("VSync_Toggle").GetComponent<Toggle>();
#endif
        setting_Open_Btn = GameObject.Find("SettingOpenBtn").GetComponent<Button>();
        setting_Close_Btn = GameObject.Find("SettingButton_Close").GetComponent<Button>();
        mod_Open_Btn = GameObject.Find("ModOpenBtn").GetComponent<Button>();
        mod_Close_Btn = GameObject.Find("ModButton_Close").GetComponent<Button>();

        syncLine_Tog = GameObject.Find("Sync_Toggle").GetComponent<Toggle>();
        offBeat_Tog = GameObject.Find("Offbeat_Toggle").GetComponent<Toggle>();
        mirrow_Tog = GameObject.Find("Mirrow_Toggle").GetComponent<Toggle>();
        persp_Tog = GameObject.Find("Perspective_Toggle").GetComponent<Toggle>();

        noteToggles = GameObject.Find("Note_Group").GetComponent<NoteStyleToggleGroup>();
        seSelector = GameObject.Find("SEGroup").GetComponent<SESelector>();
        Video_Tog = GameObject.Find("Video_Toggle").GetComponent<Toggle>();

        speed_Input = GameObject.Find("Speed_Input").GetComponent<InputField>();
        judge_Input = GameObject.Find("Judge_Input").GetComponent<InputField>();
        audio_Input = GameObject.Find("Audio_Input").GetComponent<InputField>();
        size_Input = GameObject.Find("Size_Input").GetComponent<InputField>();

        judgeOffsetTransform = GameObject.Find("Judge_Offset_Transform_Slider").GetComponent<Slider>();
        far_Clip = GameObject.Find("Far_Clip_Slider").GetComponent<Slider>();
        bg_Bright = GameObject.Find("BG_Bri_Slider").GetComponent<Slider>();
        lane_Bright = GameObject.Find("Lane_Bri_Slider").GetComponent<Slider>();
        long_Bright = GameObject.Find("Long_Bri_Slider").GetComponent<Slider>();
        seVolume_Input = GameObject.Find("SeVolume_Input").GetComponent<Slider>();
        igseVolume_Input = GameObject.Find("IGSEVolume_Input").GetComponent<Slider>();
        bgmVolume_Input = GameObject.Find("BGMVolume_Input").GetComponent<Slider>();
        ELP_Slider = GameObject.Find("ELP_Slider").GetComponent<Slider>();

        auto_Tog = GameObject.Find("Autoplay_Toggle").GetComponent<Toggle>();
        speedDown_Tog = GameObject.Find("Half_Toggle").GetComponent<StepToggle>();
        speedUp_Tog = GameObject.Find("Double_Toggle").GetComponent<StepToggle>();
        suddenDeath_Tog = GameObject.Find("SuddenDeath_Toggle").GetComponent<Toggle>();
        perfect_Tog = GameObject.Find("Perfect_Toggle").GetComponent<Toggle>();

        milisec_Tog = GameObject.Find("Milisec_Toggle").GetComponent<Toggle>();
        laneLight_Tog = GameObject.Find("LaneLight_Toggle").GetComponent<Toggle>();
        shake_Tog = GameObject.Find("Shake_Toggle").GetComponent<Toggle>();

        language_Dropdown = GameObject.Find("Language_Dropdown").GetComponent<Dropdown>();
        language_Dropdown.onValueChanged.AddListener(OnLanuageChanged);

        setting_Open_Btn.onClick.AddListener(OpenSetting);
        setting_Close_Btn.onClick.AddListener(CloseSetting);
        mod_Open_Btn.onClick.AddListener(OpenMod);
        mod_Close_Btn.onClick.AddListener(CloseMod);

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
            if (float.Parse(size_Input.text) < 0.1f) { size_Input.text = "3"; }
            if (float.Parse(size_Input.text) > 2f) { size_Input.text = "0.1"; }
            size_Input.text = string.Format("{0:F1}", float.Parse(size_Input.text));
        });

        GameObject.Find("JudOff>").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) + 1f).ToString(); });
        GameObject.Find("JudOff<").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) - 1f).ToString(); });

        GameObject.Find("AudOff>").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) + 1f).ToString(); });
        GameObject.Find("AudOff<").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) - 1f).ToString(); });
        //live setting init

    }

    void OpenSetting()
    {
        //GetModStatus();
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().Play("DropDown");
        if (GameObject.Find("Sound_Panel") != null)
        {
            SelectManager.instance.previewSound?.Pause();
        }
    }
    void CloseSetting()
    {
        SelectManager.instance.previewSound.Play();
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().Play("FlyUp");
        SetLiveSetting();
        File.WriteAllText(LiveSetting.settingsPath, JsonConvert.SerializeObject(new LiveSettingTemplate()));

        AudioManager.Provider.SetSoundEffectVolume(LiveSetting.seVolume, SEType.Common);
        AudioManager.Provider.SetSoundEffectVolume(LiveSetting.igseVolume, SEType.InGame);
        AudioManager.Provider.SetSoundTrackVolume(LiveSetting.bgmVolume);
    }
    void OpenMod()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().Play("ModDropDown");
    }
    void CloseMod()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().Play("ModFlyUp");
        SetLiveSetting();
    }
    void GetLiveSetting()
    {
        if (SelectManager.letTheBassKick)
        {
            LiveSetting.farClip = 169f;
        }
        speed_Input.text = LiveSetting.noteSpeed.ToString();
        judge_Input.text = LiveSetting.judgeOffset.ToString();
        audio_Input.text = LiveSetting.audioOffset.ToString();
        size_Input.text = LiveSetting.noteSize.ToString();
        syncLine_Tog.isOn = LiveSetting.syncLineEnabled;
        offBeat_Tog.isOn = LiveSetting.grayNoteEnabled;
        mirrow_Tog.isOn = LiveSetting.mirrowEnabled;
        persp_Tog.isOn = LiveSetting.bangPerspective;
        ELP_Slider.value = LiveSetting.ELPValue;

        laneLight_Tog.isOn = LiveSetting.laneLight;
        shake_Tog.isOn = LiveSetting.shakeFlick;
        milisec_Tog.isOn = LiveSetting.dispMilisec;
        Video_Tog.isOn = LiveSetting.useVideo;

        judgeOffsetTransform.value = LiveSetting.offsetTransform;
        far_Clip.value = LiveSetting.farClip;
        bg_Bright.value = LiveSetting.bgBrightness;
        lane_Bright.value = LiveSetting.laneBrightness;
        long_Bright.value = LiveSetting.longBrightness;

        seVolume_Input.value = LiveSetting.seVolume;
        igseVolume_Input.value = LiveSetting.igseVolume;
        bgmVolume_Input.value = LiveSetting.bgmVolume;

        noteToggles.SetStyle(LiveSetting.noteStyle);
        seSelector.SetSE(LiveSetting.seStyle);
        language_Dropdown.value = (int)LiveSetting.language;
#if (UNITY_STANDALONE || UNITY_WSA)
        FS_Tog.isOn = Screen.fullScreen;
        VSync_Tog.isOn = QualitySettings.vSyncCount == 1;
#endif
        GetModStatus();
    }
    void GetModStatus()
    {
        auto_Tog.isOn = LiveSetting.autoPlayEnabled;

        speedDown_Tog.SetStep(LiveSetting.attachedMods);
        speedUp_Tog.SetStep(LiveSetting.attachedMods);
        suddenDeath_Tog.isOn = LiveSetting.attachedMods.Contains(SuddenDeathMod.Instance);
        perfect_Tog.isOn = LiveSetting.attachedMods.Contains(PerfectMod.Instance);
    }

    public void OnLanuageChanged(int value)
    {
        LiveSetting.language = (Language)value;
        LocalizedStrings.Instanse.ReloadLanguageFile(LiveSetting.language);
        LocalizedText.ReloadAll();
    }

    public void SetLiveSetting()
    {
        try
        {
            LiveSetting.noteSpeed = float.Parse(speed_Input.text);
            LiveSetting.judgeOffset = int.Parse(string.IsNullOrWhiteSpace(judge_Input.text) ? 0.ToString() : judge_Input.text);
            LiveSetting.audioOffset = int.Parse(string.IsNullOrWhiteSpace(audio_Input.text) ? 0.ToString() : audio_Input.text);
            LiveSetting.noteSize = float.Parse(size_Input.text);
            LiveSetting.seVolume = seVolume_Input.value;
            LiveSetting.igseVolume = igseVolume_Input.value;
            LiveSetting.bgmVolume = bgmVolume_Input.value;
            LiveSetting.syncLineEnabled = syncLine_Tog.isOn;
            LiveSetting.grayNoteEnabled = offBeat_Tog.isOn;
            LiveSetting.mirrowEnabled = mirrow_Tog.isOn;
            LiveSetting.autoPlayEnabled = auto_Tog.isOn;
            LiveSetting.bangPerspective = persp_Tog.isOn;
            LiveSetting.ELPValue = ELP_Slider.value;
            LiveSetting.laneLight = laneLight_Tog.isOn;
            LiveSetting.shakeFlick = shake_Tog.isOn;
            LiveSetting.dispMilisec = milisec_Tog.isOn;
            LiveSetting.useVideo = Video_Tog.isOn;
#if (UNITY_STANDALONE || UNITY_WSA)
            if (FS_Tog.isOn)
            {
                var r = Screen.resolutions[Screen.resolutions.Length - 1];
                Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
            }
            else
            {
                var r = Screen.resolutions[Screen.resolutions.Length - 2];
                Screen.SetResolution(r.width, r.height, FullScreenMode.Windowed);
                Screen.fullScreen = false;
            }

            if(VSync_Tog.isOn)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
#endif
            LiveSetting.offsetTransform = judgeOffsetTransform.value;
            LiveSetting.farClip = far_Clip.value;
            LiveSetting.bgBrightness = bg_Bright.value;
            LiveSetting.laneBrightness = lane_Bright.value;
            LiveSetting.longBrightness = long_Bright.value;

            LiveSetting.noteStyle = noteToggles.GetStyle();
            LiveSetting.seStyle = seSelector.GetSE();

            LiveSetting.RemoveAllMods();
            LiveSetting.attachedMods.Clear();
            LiveSetting.AddMod(speedUp_Tog.GetStep());
            LiveSetting.AddMod(speedDown_Tog.GetStep());

            if (suddenDeath_Tog.isOn) LiveSetting.AddMod(SuddenDeathMod.Instance);

            if (perfect_Tog.isOn) LiveSetting.AddMod(PerfectMod.Instance);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e.Message);
            throw e;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        initCompoinents();
        GetLiveSetting();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
