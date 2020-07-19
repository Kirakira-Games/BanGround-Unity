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
        engineTgs = GetComponentsInChildren<Toggle>(true);

        KVarRef snd_engine = new KVarRef("snd_engine");

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
