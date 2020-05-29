﻿using System.Collections;
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

    /*
     * Config KVars
    */

    // o for Offset
    static KVar o_judge = new KVar("o_judge", "0", KVarFlags.Archive);
    static KVar o_audio = new KVar("o_audio", "0", KVarFlags.Archive);

    // snd for Sound
    static KVarRef snd_bgm_volume = new KVarRef("snd_bgm_volume");
    static KVarRef snd_se_volume = new KVarRef("snd_se_volume");
    static KVarRef snd_igse_volume = new KVarRef("snd_igse_volume");

    // r for Render
    static KVar r_notespeed = new KVar("r_notespeed", "10.0", KVarFlags.Archive);
    static KVar r_notesize = new KVar("r_notesize", "1.0", KVarFlags.Archive);

    static KVar r_syncline = new KVar("r_syncline", "1", KVarFlags.Archive);
    static KVar r_lanefx = new KVar("r_lanefx", "1", KVarFlags.Archive);
    static KVar r_graynote = new KVar("r_graynote", "1", KVarFlags.Archive);
    static KVar r_mirror = new KVar("r_mirror", "0", KVarFlags.Archive);
    static KVar r_bang_perspect = new KVar("r_bang_perspect", "1", KVarFlags.Archive);
    static KVar r_shake_flick = new KVar("r_shake_flick", "1", KVarFlags.Archive);

    static KVar r_usevideo = new KVar("r_usevideo", "1", KVarFlags.Archive);

    static KVar r_farclip = new KVar("r_farclip", "196.0", KVarFlags.Archive);
    static KVar r_brightness_bg = new KVar("r_brightness_bg", "0.7", KVarFlags.Archive);
    static KVar r_brightness_lane = new KVar("r_brightness_lane", "0.84", KVarFlags.Archive);
    static KVar r_brightness_long = new KVar("r_brightness_long", "0.8", KVarFlags.Archive);

    // cl for Client
    static KVar cl_showms = new KVar("cl_showms", "0", KVarFlags.Archive);
    static KVar cl_elp = new KVar("cl_elp", "0", KVarFlags.Archive);
    static KVar cl_offset_transform = new KVar("cl_offset_transform", "1", KVarFlags.Archive);

    static KVar cl_notestyle = new KVar("cl_notestyle", "0", KVarFlags.Archive);
    static KVar cl_sestyle = new KVar("cl_sestyle", "1", KVarFlags.Archive);

    static KVarRef cl_language = new KVarRef("cl_language");

    // mod for Mod
    static KVar mod_autoplay = new KVar("mod_autoplay", "0");
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
        if (GameObject.Find("SoundToggle").GetComponent<Toggle>().isOn)
        {
            SelectManager_old.instance.previewSound?.Pause();
        }
    }
    void CloseSetting()
    {
        
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetTrigger("SwitchSetting");
        SetLiveSetting();
        setting_Close_Btn.gameObject.SetActive(false);
        KVSystem.Instance.SaveConfig();
        SelectManager_old.instance.previewSound.Play();
        AudioManager.Provider.SetSoundEffectVolume(snd_se_volume, SEType.Common);
        AudioManager.Provider.SetSoundEffectVolume(snd_igse_volume, SEType.InGame);
        AudioManager.Provider.SetSoundTrackVolume(snd_bgm_volume);
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
        mirrow_Tog.isOn = r_mirror;
        persp_Tog.isOn = r_bang_perspect;
        ELP_Slider.value = cl_elp;

        laneLight_Tog.isOn = r_lanefx;
        shake_Tog.isOn = r_shake_flick;
        milisec_Tog.isOn = cl_showms;
        Video_Tog.isOn = r_usevideo;

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
        auto_Tog.isOn = mod_autoplay;

        speedDown_Tog.SetStep(LiveSetting.attachedMods);
        speedUp_Tog.SetStep(LiveSetting.attachedMods);
        suddenDeath_Tog.isOn = LiveSetting.attachedMods.Contains(SuddenDeathMod.Instance);
        perfect_Tog.isOn = LiveSetting.attachedMods.Contains(PerfectMod.Instance);
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
            r_mirror.Set(mirrow_Tog.isOn);
            mod_autoplay.Set(auto_Tog.isOn);
            r_bang_perspect.Set(persp_Tog.isOn);
            cl_elp.Set(ELP_Slider.value);
            r_lanefx.Set(laneLight_Tog.isOn);
            r_shake_flick.Set(shake_Tog.isOn);
            cl_showms.Set(milisec_Tog.isOn);
            r_usevideo.Set(Video_Tog.isOn);
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

}
