using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchEngine : MonoBehaviour
{
    ToggleGroup tg;
    Toggle[] engineTgs;

    void Start()
    {
        tg = GetComponent<ToggleGroup>();
        engineTgs = GetComponentsInChildren<Toggle>();

        engineTgs[0].onValueChanged.AddListener(on =>
        {
            if (on) PlayerPrefs.SetString("AudioEngine", "Bass");
        });

        engineTgs[1].onValueChanged.AddListener(on =>
        {
            if (on) PlayerPrefs.SetString("AudioEngine", "Fmod");
        });

        bool bass = PlayerPrefs.GetString("AudioEngine", "Bass") == "Bass";
        engineTgs[0].isOn = bass;
        engineTgs[1].isOn = !bass;
    }
}
