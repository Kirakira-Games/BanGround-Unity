using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleValue_buffer : MonoBehaviour
{
    private Slider slider;
    private Text valueText;
    public static float[] bufferSize = new float[] { 0.125f, 0.25f, 0.5f, 1, 2, 4, 8 };

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();

        valueText.text = (AppPreLoader.bufferSize * bufferSize[(int)slider.value]).ToString();
        slider.onValueChanged.AddListener((value) =>
        {
            valueText.text = (AppPreLoader.bufferSize * bufferSize[(int)slider.value]).ToString();
        });
    }

}
