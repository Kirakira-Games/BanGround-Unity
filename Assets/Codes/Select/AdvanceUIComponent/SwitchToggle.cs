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

    private SelectManager sm;

    void Start()
    {
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        toggle = GetComponent<Toggle>();
        panelObject = GameObject.Find(panel);
        toggle.onValueChanged.AddListener((active) =>
        {
            panelObject.SetActive(active);
            if(panel == "Sound_Panel")
            {
                if (active) sm.previewSound?.Pause();
                else sm.previewSound?.Play();
            }
        });

        panelObject.SetActive(init);
    }
}
