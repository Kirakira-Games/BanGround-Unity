using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//挂在了相机上
public class SelectManager : MonoBehaviour
{
    private Button enter_Btn;
    private ToggleGroup selectGroup;

    private InputField speed_Input;
    private InputField judge_Input;
    private InputField audio_Input;

    // Start is called before the first frame update
    void Start()
    {
        enter_Btn = GameObject.Find("Enter_Btn").GetComponent<Button>();
        selectGroup = GameObject.Find("Select_Group").GetComponent<ToggleGroup>();

        speed_Input = GameObject.Find("Speed_Input").GetComponent<InputField>();
        judge_Input = GameObject.Find("Judge_Input").GetComponent<InputField>();
        audio_Input = GameObject.Find("Audio_Input").GetComponent<InputField>();

        speed_Input.text = LiveSetting.noteSpeed.ToString();
        judge_Input.text = LiveSetting.judgeOffset.ToString();
        audio_Input.text = LiveSetting.audioOffset.ToString();

        enter_Btn.onClick.AddListener(() =>
        {
            var toggles = selectGroup.ActiveToggles();
            foreach (var seleted in toggles)
            {
                //Debug.Log(seleted.name);
                LiveSetting.selected = seleted.name;
            }

            LiveSetting.noteSpeed = float.Parse(speed_Input.text);
            LiveSetting.judgeOffset = int.Parse(judge_Input.text);
            LiveSetting.audioOffset = int.Parse(audio_Input.text);

            SceneManager.LoadScene("InGame");
        });

    }

}
