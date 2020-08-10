using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SwitchEngine : MonoBehaviour
{
    ToggleGroup tg;
    Toggle[] engineTgs;

    [Inject(Id = "snd_engine")]
    KVar snd_engine;

    void Start()
    {
        tg = GetComponent<ToggleGroup>();
        engineTgs = GetComponentsInChildren<Toggle>(true);

        engineTgs[0].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Bass");
        });

        engineTgs[1].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Fmod");
        });

        engineTgs[2].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Unity");
        });

        engineTgs[0].isOn = snd_engine == "Bass";
        engineTgs[1].isOn = snd_engine == "Fmod";
        engineTgs[2].isOn = snd_engine == "Unity";

    }
}
