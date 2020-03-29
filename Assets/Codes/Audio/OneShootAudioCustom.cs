using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShootAudioCustom : MonoBehaviour
{

    [SerializeField] private TextAsset sound;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PrecacheSE(sound.bytes).PlayOneShot();

    }

}
