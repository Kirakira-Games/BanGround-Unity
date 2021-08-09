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
        /*
        engineTgs[2].onValueChanged.AddListener(on =>
        {
            if (on) snd_engine.Set("Unity");
        });*/
        switch ((string) snd_engine)
        {
            case "Bass":
                engineTgs[0].isOn = true;
                break;
            case "Fmod":
                engineTgs[1].isOn = true;
                break;
            default:
                Debug.LogError("Unrecognized sound engine: " + snd_engine);
                break;
        }
    }
}
