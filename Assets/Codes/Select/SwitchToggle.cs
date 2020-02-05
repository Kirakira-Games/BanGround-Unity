using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SwitchToggle : MonoBehaviour
{
    private Toggle toggle;
    private GameObject panelObject;
    [SerializeField] private string panel;
    [SerializeField] private bool init = false;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        panelObject = GameObject.Find(panel);
        toggle.onValueChanged.AddListener((active) =>
        {
            panelObject.SetActive(active);
        });

        panelObject.SetActive(init);
    }
}
