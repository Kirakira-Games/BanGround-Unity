using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandelValue_Buffer : MonoBehaviour
{
    public static readonly int[] BufferScale = { 8, 6, 4, 2, 1 };

    private Slider slider;
    private Text valueText;

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();
        slider.value = PlayerPrefs.GetInt("BufferIndex", -1);

        valueText.text = (AppPreLoader.bufferSize / BufferScale[(int)slider.value]).ToString();
        slider.onValueChanged.AddListener((value) =>
        {
            valueText.text = (AppPreLoader.bufferSize / BufferScale[(int)value]).ToString();
            PlayerPrefs.SetInt("BufferIndex", (int)value);
        });

    }
}
