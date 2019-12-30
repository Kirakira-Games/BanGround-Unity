using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    Text text;
    int frameInSec = 0;
    float lastClearTime = -114514;

    void Awake()
    {
        text = GetComponent<Text>();
    }
    void Update()
    {
        if(Time.time - lastClearTime > 1)
        {
            text.text = $"{frameInSec} fps\n{Mathf.Round(Time.deltaTime * 1000)} ms";
            frameInSec = 0;
            lastClearTime = Time.time;
        }

        frameInSec++;
    }
}
