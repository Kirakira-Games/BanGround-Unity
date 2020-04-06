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

        bool fmod = PlayerPrefs.GetString("AudioEngine", "Fmod") == "Fmod";
        engineTgs[0].isOn = !fmod;
        engineTgs[1].isOn = fmod;

    }
}
