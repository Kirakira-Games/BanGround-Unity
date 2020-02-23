using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleValue_buffer : MonoBehaviour
{
    private Slider slider;
    private Text valueText;
    public static float[] bufferSize = new float[] { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2 };

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
