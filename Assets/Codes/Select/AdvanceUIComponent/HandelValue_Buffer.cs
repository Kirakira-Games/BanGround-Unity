using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandelValue_Buffer : MonoBehaviour
{
    public static readonly int[] FmodBufferScale = { 8, 6, 4, 2, 1 };
    public static readonly float[] BassBufferScale = { .5f, .6f, .7f, .8f, .9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };

    private Slider slider;
    private Text valueText;

    void Start()
    {
        slider = GetComponent<Slider>();
        valueText = transform.Find("Handle Slide Area/Handle/Value").GetComponent<Text>();

        if (Application.platform != RuntimePlatform.Android)
        {
            GameObject.Find("BufferSize_Text").SetActive(false);
            GameObject.Find("BufferSize_Input").SetActive(false);
            return;
        }

        KVarRef cl_audioengine = new KVarRef("cl_audioengine");
        KVarRef cl_bassbuffer = new KVarRef("cl_bassbuffer");
        KVarRef cl_fmodbuffer = new KVarRef("cl_fmodbuffer");

        if (cl_audioengine == "Bass")
        {
            slider.minValue = 0;
            slider.maxValue = 15;
            slider.wholeNumbers = true;

            slider.value = cl_bassbuffer;
            valueText.text = ((int)(AppPreLoader.bufferSize * BassBufferScale[(int)slider.value])).ToString();
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = ((int)(AppPreLoader.bufferSize * BassBufferScale[(int)slider.value])).ToString();
                cl_bassbuffer.Set((int)value);
            });
        }
        else
        {
            slider.minValue = 0;
            slider.maxValue = 4;
            slider.wholeNumbers = true;

            slider.value = cl_fmodbuffer;
            valueText.text = (AppPreLoader.bufferSize / FmodBufferScale[(int)slider.value]).ToString();
            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = (AppPreLoader.bufferSize / FmodBufferScale[(int)value]).ToString();
                cl_fmodbuffer.Set((int)value);
            });
        }
    }
}
