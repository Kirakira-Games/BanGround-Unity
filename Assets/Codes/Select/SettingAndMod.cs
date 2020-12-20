using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioProvider;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using BanGround.Game.Mods;

public class SettingAndMod : MonoBehaviour
{
    public static SettingAndMod instance;

    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IKVSystem kvSystem;
    [Inject(Id = "cl_modflag")]
    private KVar cl_modflag;

    private Button setting_Open_Btn;
    private Button setting_Close_Btn;
    private Button mod_Open_Btn;
    private Button mod_Close_Btn;
    private Button Open_LunarConsole;

    private Toggle syncLine_Tog;
    private Toggle offBeat_Tog;
    private Toggle persp_Tog;
    private Slider ELP_Slider;
    private Toggle FS_Tog;
    private Toggle VSync_Tog;
    private Toggle laneLight_Tog;
    private Toggle shake_Tog;
    private Toggle milisec_Tog;
    private Toggle Video_Tog;
    private Toggle Resolution_Tog;
    private NoteStyleToggleGroup noteToggles;
    private SESelector seSelector;

    public Toggle soundTog;

    /* Mods     */

    [Header("Mod toggles")]
    // Auto
    public Toggle autoToggle;
    // Double
    public StepToggle speedUpToggle;
    // Half
    public StepToggle speedDownToggle;
    // Sudden Death
    public Toggle suddenDeathToggle;
    // Perfect
    public Toggle perfectToggle;
    // Mirror
    public Toggle mirrorToggle;

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

    /*
     * Config KVars
    */

    // o for Offset
    [Inject(Id = "o_judge")]
    KVar o_judge;
    [Inject(Id = "o_audio")]
    KVar o_audio;

    // snd for Sound
    [Inject(Id = "snd_bgm_volume")]
    KVar snd_bgm_volume;
    [Inject(Id = "snd_se_volume")]
    KVar snd_se_volume;
    [Inject(Id = "snd_igse_volume")]
    KVar snd_igse_volume;

    // r for Render
    [Inject(Id = "r_notespeed")]
    KVar r_notespeed;
    [Inject(Id = "r_notesize")]
    KVar r_notesize;

    [Inject(Id = "r_syncline")]
    KVar r_syncline;
    [Inject(Id = "r_lanefx")]
    KVar r_lanefx;
    [Inject(Id = "r_graynote")]
    KVar r_graynote;
    [Inject(Id = "r_bang_perspect")]
    KVar r_bang_perspect;
    [Inject(Id = "r_shake_flick")]
    KVar r_shake_flick;

    [Inject(Id = "r_usevideo")]
    KVar r_usevideo;

    [Inject(Id = "r_farclip")]
    KVar r_farclip;
    [Inject(Id = "r_brightness_bg")]
    KVar r_brightness_bg;
    [Inject(Id = "r_brightness_lane")]
    KVar r_brightness_lane;
    [Inject(Id = "r_brightness_long")]
    KVar r_brightness_long;
    [Inject(Id = "r_lowresolution")]
    KVar r_lowresolution;

    // cl for Client
    [Inject(Id = "cl_showms")]
    KVar cl_showms;
    [Inject(Id = "cl_elp")]
    KVar cl_elp;
    [Inject(Id = "cl_offset_transform")]
    KVar cl_offset_transform;

    [Inject(Id = "cl_notestyle")]
    KVar cl_notestyle;
    [Inject(Id = "cl_sestyle")]
    KVar cl_sestyle;

    [Inject(Id = "cl_language")]
    KVar cl_language;

    /*
     * End
     */
    void initCompoinents()
    {
#if !(UNITY_STANDALONE || UNITY_WSA)
        GameObject.Find("Windows_Panel").SetActive(false);
#else
        FS_Tog = GameObject.Find("Fullscreen_Toggle").GetComponent<Toggle>();
        VSync_Tog = GameObject.Find("VSync_Toggle").GetComponent<Toggle>();
#endif
        setting_Open_Btn = GameObject.Find("SettingOpenBtn").GetComponent<Button>();
        setting_Close_Btn = GameObject.Find("SettingButton_Close").GetComponent<Button>();
        mod_Open_Btn = GameObject.Find("ModOpenBtn").GetComponent<Button>();
        mod_Close_Btn = GameObject.Find("ModButton_Close").GetComponent<Button>();
        Open_LunarConsole = GameObject.Find("OpenConsole").GetComponent<Button>();//

        syncLine_Tog = GameObject.Find("Sync_Toggle").GetComponent<Toggle>();
        offBeat_Tog = GameObject.Find("Offbeat_Toggle").GetComponent<Toggle>();
        persp_Tog = GameObject.Find("Perspective_Toggle").GetComponent<Toggle>();

        noteToggles = GameObject.Find("Note_Group").GetComponent<NoteStyleToggleGroup>();
        seSelector = GameObject.Find("SEGroup").GetComponent<SESelector>();
        Video_Tog = GameObject.Find("Video_Toggle").GetComponent<Toggle>();
        Resolution_Tog = GameObject.Find("Resolution_Tog").GetComponent<Toggle>();

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
            if (float.Parse(size_Input.text) > 3f) { size_Input.text = "0.1"; }
            size_Input.text = string.Format("{0:F1}", float.Parse(size_Input.text));
        });

        GameObject.Find("JudOff>").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) + 1f).ToString(); });
        GameObject.Find("JudOff<").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) - 1f).ToString(); });

        GameObject.Find("AudOff>").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) + 1f).ToString(); });
        GameObject.Find("AudOff<").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) - 1f).ToString(); });
        //live setting init

        

        setting_Close_Btn.gameObject.SetActive(false);
    }

    void OpenSetting()
    {
        //GetModStatus();
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetTrigger("SwitchSetting");
        setting_Close_Btn.gameObject.SetActive(true);
        if (soundTog.isOn)
        {
            SelectManager_old.instance.previewSound?.Pause();
        }
    }
    void CloseSetting()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetTrigger("SwitchSetting");
        SetLiveSetting();
        setting_Close_Btn.gameObject.SetActive(false);
        kvSystem.SaveConfig();
        SelectManager_old.instance.previewSound?.Play();
    }
    void OpenMod()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetTrigger("SwitchMod");
    }
    void CloseMod()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetTrigger("SwitchMod");
        SetLiveSetting();
    }
    void GetLiveSetting()
    {
        speed_Input.text = r_notespeed;
        judge_Input.text = o_judge;
        audio_Input.text = o_audio;
        size_Input.text = r_notesize;
        syncLine_Tog.isOn = r_syncline;
        offBeat_Tog.isOn = r_graynote;
        persp_Tog.isOn = r_bang_perspect;
        ELP_Slider.value = cl_elp;

        laneLight_Tog.isOn = r_lanefx;
        shake_Tog.isOn = r_shake_flick;
        milisec_Tog.isOn = cl_showms;
        Video_Tog.isOn = r_usevideo;
        Resolution_Tog.isOn = r_lowresolution;

        judgeOffsetTransform.value = cl_offset_transform;
        far_Clip.value = r_farclip;
        bg_Bright.value = r_brightness_bg;
        lane_Bright.value = r_brightness_lane;
        long_Bright.value = r_brightness_long;

        seVolume_Input.value = snd_se_volume;
        igseVolume_Input.value = snd_igse_volume;
        bgmVolume_Input.value = snd_bgm_volume;

        noteToggles.SetStyle((NoteStyle)cl_notestyle);
        seSelector.SetSE((SEStyle)cl_sestyle);
        language_Dropdown.value = cl_language;
#if (UNITY_STANDALONE || UNITY_WSA)
        FS_Tog.isOn = Screen.fullScreen;
        VSync_Tog.isOn = QualitySettings.vSyncCount == 1;
#endif
        GetModStatus();
    }
    void GetModStatus()
    {
        var flag = ModFlagUtil.From(cl_modflag);

        autoToggle.isOn = flag.HasFlag(ModFlag.AutoPlay);
        speedDownToggle.SetStep(flag);
        speedUpToggle.SetStep(flag);
        suddenDeathToggle.isOn = flag.HasFlag(ModFlag.SuddenDeath);
        perfectToggle.isOn = flag.HasFlag(ModFlag.Perfect);
        mirrorToggle.isOn = flag.HasFlag(ModFlag.Mirror);
    }

    public void OnLanuageChanged(int value)
    {
        cl_language.Set(value);
        LocalizedStrings.Instanse.ReloadLanguageFile(cl_language);
        LocalizedText.ReloadAll();
    }

    public void SetLiveSetting()
    {
        try
        {
            r_notespeed.Set(speed_Input.text);
            o_judge.Set(string.IsNullOrWhiteSpace(judge_Input.text) ? 0.ToString() : judge_Input.text);
            o_audio.Set(string.IsNullOrWhiteSpace(audio_Input.text) ? 0.ToString() : audio_Input.text);
            r_notesize.Set(size_Input.text);
            snd_se_volume.Set(seVolume_Input.value);
            snd_igse_volume.Set(igseVolume_Input.value);
            snd_bgm_volume.Set(bgmVolume_Input.value);
            r_syncline.Set(syncLine_Tog.isOn);
            r_graynote.Set(offBeat_Tog.isOn);
            r_bang_perspect.Set(persp_Tog.isOn);
            cl_elp.Set(ELP_Slider.value);
            r_lanefx.Set(laneLight_Tog.isOn);
            r_shake_flick.Set(shake_Tog.isOn);
            cl_showms.Set(milisec_Tog.isOn);
            r_usevideo.Set(Video_Tog.isOn);
            r_lowresolution.Set(Resolution_Tog.isOn);
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
            cl_offset_transform.Set(judgeOffsetTransform.value);
            r_farclip.Set(far_Clip.value);
            r_brightness_bg.Set(bg_Bright.value);
            r_brightness_lane.Set(lane_Bright.value);
            r_brightness_long.Set(long_Bright.value);

            cl_notestyle.Set((int)noteToggles.GetStyle());
            cl_sestyle.Set((int)seSelector.GetSE());

            ModFlag flag = ModFlag.None;
            flag |= speedUpToggle.GetStep();
            flag |= speedDownToggle.GetStep();

            if (suddenDeathToggle.isOn)
                flag |= ModFlag.SuddenDeath;

            if (perfectToggle.isOn)
                flag |= ModFlag.Perfect;

            if (autoToggle.isOn)
                flag |= ModFlag.AutoPlay;

            if (mirrorToggle.isOn)
                flag |= ModFlag.Mirror;

            cl_modflag.SetMod(flag);
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
        Open_LunarConsole.onClick.AddListener(() => { LunarConsolePlugin.LunarConsole.Show(); });
    }
}
   
