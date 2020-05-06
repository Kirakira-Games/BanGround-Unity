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

        KVarRef snd_engine = new KVarRef("snd_engine");

        engineTgs[0].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Bass");
        });

        engineTgs[1].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Fmod");
        });

        bool fmod = snd_engine == "Fmod";
        engineTgs[0].isOn = !fmod;
        engineTgs[1].isOn = fmod;

    }
}
