using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HandelValue : MonoBehaviour
{
    [Inject]
    LocalizedStrings localizedStrings;

    private Slider slider;
    private Text valueText;
    public bool needPercent = true;
    public bool canDisable = false;

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();
        if (needPercent)
        {
            //valueText.text = ((int)(slider.value * 100)).ToString() + "%";
            //slider.onValueChanged.AddListener((value) =>
            //{
            //    valueText.text = ((int)(value * 100)).ToString() + "%";
            //});
            valueText.text = (int)((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * 100) + "%";
            if (slider.value == 0) valueText.text = localizedStrings.GetLocalizedString("Disable");
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = (int)((value - slider.minValue) / (slider.maxValue - slider.minValue) * 100) + "%";
                if (canDisable)
                {
                    if (slider.value == 0) valueText.text = localizedStrings.GetLocalizedString("Disable");
                }
            });
        }
        else
        {
            valueText.text = ((int)(slider.value)).ToString();
            if (slider.value == 0) valueText.text = localizedStrings.GetLocalizedString("Disable");
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = ((int)(value)).ToString();
                if (canDisable)
                {
                    if (slider.value == 0) valueText.text = localizedStrings.GetLocalizedString("Disable");
                }
            });
        }

    }

}
