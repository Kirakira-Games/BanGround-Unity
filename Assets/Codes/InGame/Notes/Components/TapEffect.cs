using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapEffect : MonoBehaviour
{
    private ParticleSystem effect;
    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }
    public void OnUpdate()
    {
        if (transform.parent.gameObject.activeSelf)
        {
            if (!effect.isPlaying)
                effect.Play();
        }
        else
        {
            if (effect.isPlaying)
                effect.Pause();
        }
    }
}
