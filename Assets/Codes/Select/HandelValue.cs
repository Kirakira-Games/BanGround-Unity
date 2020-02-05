using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandelValue : MonoBehaviour
{
    private Slider slider;
    private Text valueText;

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();

        valueText.text = ((int)(slider.value * 100)).ToString() + "%";
        slider.onValueChanged.AddListener((value) =>
        {
            valueText.text = ((int)(value * 100)).ToString() + "%";
        });
    }

}
