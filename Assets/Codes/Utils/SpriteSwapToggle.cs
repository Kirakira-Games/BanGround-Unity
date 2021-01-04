using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Toggle))]
public class SpriteSwapToggle : MonoBehaviour
{
    public Image Background;

    private void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(on =>
        {
            Background.enabled = !on;
            toggle.graphic.enabled = on;
        });
    }
}
