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

        KVarRef cl_audioengine = new KVarRef("cl_audioengine");

        engineTgs[0].onValueChanged.AddListener(on =>
        {
            if (on) cl_audioengine.Set("Bass");
        });

        engineTgs[1].onValueChanged.AddListener(on =>
        {
            if (on) cl_audioengine.Set("Fmod");
        });

        bool fmod = cl_audioengine == "Fmod";
        engineTgs[0].isOn = !fmod;
        engineTgs[1].isOn = fmod;

    }
}
