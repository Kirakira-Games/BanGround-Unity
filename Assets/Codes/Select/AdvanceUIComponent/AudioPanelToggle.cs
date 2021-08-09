using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using Zenject;

#pragma warning disable 0649
[RequireComponent(typeof(Toggle))]
public class AudioPanelToggle : MonoBehaviour
{
    private Toggle toggle;
    private Animator animator;

    [Inject]
    SelectManager selectManager;

    private void OnToggle(bool active)
    {
        if (active)
            selectManager.previewSound?.Pause();
        else
            selectManager.previewSound?.Play();
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dropped"))
            return;
    }

    private void Awake()
    {
        animator = GameObject.Find("Setting_Canvas").GetComponent<Animator>();
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }
}
