using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleValue_buffer : MonoBehaviour
{
    private Slider slider;
    private Text valueText;
    //private readonly int[] bufferSize = new int[] { 128, 256, 512, 1024, 2048 };
    public static readonly Dictionary<int, int> bufferSize = new Dictionary<int, int> { { 0, 128 }, { 1, 256 }, { 2, 512 }, { 3, 1024 }, { 4, 2048 } };

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();

        valueText.text = bufferSize[(int)slider.value].ToString();
        slider.onValueChanged.AddListener((value) =>
        {
            valueText.text = bufferSize[(int)slider.value].ToString();
        });
    }

}
