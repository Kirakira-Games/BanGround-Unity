using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectManager : MonoBehaviour
{
    private Button enter_Btn;
    private Button setting_Open_Btn;
    private Button setting_Close_Btn;

    private ToggleGroup selectGroup;

    private Toggle syncLine_Tog;
    private Toggle offBeat_Tog;
    private Toggle auto_Tog;
    private Toggle persp_Tog;

    private InputField speed_Input;
    private InputField judge_Input;
    private InputField audio_Input;
    private InputField size_Input;


    private Slider bg_Bright;
    private Slider lane_Bright;
    private Slider seVolume_Input;

    private Animator scene_Animator;

    // Start is called before the first frame update
    void Start()
    {
        scene_Animator = GameObject.Find("SceneAnimator").GetComponent<Animator>();

        enter_Btn = GameObject.Find("Enter_Btn").GetComponent<Button>();
        setting_Open_Btn = GameObject.Find("Setting_Panel").GetComponent<Button>();
        setting_Close_Btn = GameObject.Find("Button_Close").GetComponent<Button>();

        selectGroup = GameObject.Find("Select_Group").GetComponent<ToggleGroup>();

        syncLine_Tog = GameObject.Find("Sync_Toggle").GetComponent<Toggle>();
        offBeat_Tog = GameObject.Find("Offbeat_Toggle").GetComponent<Toggle>();
        auto_Tog = GameObject.Find("Autoplay_Toggle").GetComponent<Toggle>();
        persp_Tog = GameObject.Find("Perspective_Toggle").GetComponent<Toggle>();

        speed_Input = GameObject.Find("Speed_Input").GetComponent<InputField>();
        judge_Input = GameObject.Find("Judge_Input").GetComponent<InputField>();
        audio_Input = GameObject.Find("Audio_Input").GetComponent<InputField>();
        size_Input = GameObject.Find("Size_Input").GetComponent<InputField>();

        bg_Bright = GameObject.Find("BG_Bri_Slider").GetComponent<Slider>();
        lane_Bright = GameObject.Find("Lane_Bri_Slider").GetComponent<Slider>();
        seVolume_Input = GameObject.Find("SeVolume_Input").GetComponent<Slider>();

        

        enter_Btn.onClick.AddListener(OnEnterPressed);
        setting_Open_Btn.onClick.AddListener(OpenSetting);
        setting_Close_Btn.onClick.AddListener(CloseSetting);

        GameObject.Find("Speed>").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) + 0.1f).ToString(); });
        GameObject.Find("Speed<").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) - 0.1f).ToString(); });
        GameObject.Find("Speed>>").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) + 1f).ToString(); });
        GameObject.Find("Speed<<").GetComponent<Button>().onClick.AddListener(() => { speed_Input.text = (float.Parse(speed_Input.text) - 1f).ToString(); });
        speed_Input.onValueChanged.AddListener((string a) => { 
            if (float.Parse(speed_Input.text) < 0) { speed_Input.text = "11"; } 
            if (float.Parse(speed_Input.text) > 11f) { speed_Input.text = "0"; }
            speed_Input.text = string.Format("{0:F1}", float.Parse(speed_Input.text));
        });

        GameObject.Find("Size>").GetComponent<Button>().onClick.AddListener(() => { size_Input.text = (float.Parse(size_Input.text) + 0.1f).ToString(); });
        GameObject.Find("Size<").GetComponent<Button>().onClick.AddListener(() => { size_Input.text = (float.Parse(size_Input.text) - 0.1f).ToString(); });
        size_Input.onValueChanged.AddListener((string a) => {
            if (float.Parse(size_Input.text) < 0) { size_Input.text = "2"; }
            if (float.Parse(size_Input.text) > 2f) { size_Input.text = "0"; }
            size_Input.text = string.Format("{0:F1}", float.Parse(size_Input.text));
        });

        GameObject.Find("JudOff>").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) + 1f).ToString(); });
        GameObject.Find("JudOff<").GetComponent<Button>().onClick.AddListener(() => { judge_Input.text = (float.Parse(judge_Input.text) - 1f).ToString(); });

        GameObject.Find("AudOff>").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) + 1f).ToString(); });
        GameObject.Find("AudOff<").GetComponent<Button>().onClick.AddListener(() => { audio_Input.text = (float.Parse(audio_Input.text) - 1f).ToString(); });
        
        GetLiveSetting();
    }

    bool isSettingOpened = false;
    void OpenSetting()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetBool("Drop", true);
        isSettingOpened = true;
    }
    void CloseSetting()
    {
        GameObject.Find("Setting_Canvas").GetComponent<Animator>().SetBool("Drop", false);
        isSettingOpened = false;
    }
    void GetLiveSetting()
    {
        speed_Input.text = LiveSetting.noteSpeed.ToString();
        judge_Input.text = LiveSetting.judgeOffset.ToString();
        audio_Input.text = LiveSetting.audioOffset.ToString();
        size_Input.text = LiveSetting.noteSize.ToString();
        syncLine_Tog.isOn = LiveSetting.syncLineEnabled;
        offBeat_Tog.isOn = LiveSetting.grayNoteEnabled;
        auto_Tog.isOn = LiveSetting.autoPlayEnabled;
        persp_Tog.isOn = LiveSetting.bangPerspective;

        bg_Bright.value = LiveSetting.bgBrightness;
        lane_Bright.value = LiveSetting.laneBrightness;
        seVolume_Input.value = LiveSetting.seVolume;
    }
    void SetLiveSetting()
    {
        LiveSetting.noteSpeed = float.Parse(speed_Input.text);
        LiveSetting.judgeOffset = int.Parse(judge_Input.text);
        LiveSetting.audioOffset = int.Parse(audio_Input.text);
        LiveSetting.noteSize = float.Parse(size_Input.text);
        LiveSetting.seVolume = seVolume_Input.value;
        LiveSetting.syncLineEnabled = syncLine_Tog.isOn;
        LiveSetting.grayNoteEnabled = offBeat_Tog.isOn;
        LiveSetting.autoPlayEnabled = auto_Tog.isOn;
        LiveSetting.bangPerspective = persp_Tog.isOn;

        LiveSetting.bgBrightness = bg_Bright.value;
        LiveSetting.laneBrightness = lane_Bright.value;
    }

    void OnEnterPressed()
    {
        if (!isSettingOpened)
        {
            OpenSetting();
            return;
        }
        var toggles = selectGroup.ActiveToggles();
        foreach (var seleted in toggles)
        {
            //Debug.Log(seleted.name);
            LiveSetting.selected = seleted.name;
        }

        SetLiveSetting();

        scene_Animator.Play("OutPlay", -1, 0);
        CloseSetting();
        StartCoroutine(DelayLoadScene());

    }

    IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadSceneAsync("InGame");
    }

}
