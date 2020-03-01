using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandelValue : MonoBehaviour
{
    private Slider slider;
    private Text valueText;
    public bool needPercent = true ;

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();
        if (needPercent)
        {
            valueText.text = ((int)(slider.value * 100)).ToString() + "%";
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = ((int)(value * 100)).ToString() + "%";
            });
        }
        else
        {
            valueText.text = ((int)(slider.value)).ToString();
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = ((int)(value)).ToString();
            });
        }
    }

}
