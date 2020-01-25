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

    private InputField speed_Input;
    private InputField judge_Input;
    private InputField audio_Input;
    private InputField size_Input;
    private InputField seVolume_Input;

    private Animator scene_Animator;

    // Start is called before the first frame update
    void Start()
    {
        scene_Animator = GameObject.Find("SceneAnimator").GetComponent<Animator>();

        enter_Btn = GameObject.Find("Enter_Btn").GetComponent<Button>();
        setting_Open_Btn = GameObject.Find("Setting_Panel").GetComponent<Button>();
        setting_Close_Btn = GameObject.Find("Button_Close").GetComponent<Button>();

        selectGroup = GameObject.Find("Select_Group").GetComponent<ToggleGroup>();

        speed_Input = GameObject.Find("Speed_Input").GetComponent<InputField>();
        judge_Input = GameObject.Find("Judge_Input").GetComponent<InputField>();
        audio_Input = GameObject.Find("Audio_Input").GetComponent<InputField>();
        size_Input = GameObject.Find("Size_Input").GetComponent<InputField>();
        seVolume_Input = GameObject.Find("SeVolume_Input").GetComponent<InputField>();

        
        speed_Input.text = LiveSetting.noteSpeed.ToString();
        judge_Input.text = LiveSetting.judgeOffset.ToString();
        audio_Input.text = LiveSetting.audioOffset.ToString();
        size_Input.text = LiveSetting.noteSize.ToString();
        seVolume_Input.text = LiveSetting.seVolume.ToString();

        enter_Btn.onClick.AddListener(OnEnterPressed);
        setting_Open_Btn.onClick.AddListener(OpenSetting);
        setting_Close_Btn.onClick.AddListener(CloseSetting);

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

        LiveSetting.noteSpeed = float.Parse(speed_Input.text);
        LiveSetting.judgeOffset = int.Parse(judge_Input.text);
        LiveSetting.audioOffset = int.Parse(audio_Input.text);
        LiveSetting.noteSize = float.Parse(size_Input.text);
        LiveSetting.seVolume = float.Parse(seVolume_Input.text);

        scene_Animator.Play("OutPlay", -1, 0);
        StartCoroutine(DelayLoadScene());

    }

    IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadSceneAsync("InGame");
    }

}
