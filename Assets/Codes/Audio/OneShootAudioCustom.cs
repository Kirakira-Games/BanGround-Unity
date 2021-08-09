using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class OneShootAudioCustom : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;

    [SerializeField] private TextAsset sound;
    // Start is called before the first frame update
    async void Start()
    {
        (await audioManager.PrecacheSE(sound.bytes)).PlayOneShot();

    }

}
