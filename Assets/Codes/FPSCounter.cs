using UnityEngine;
using UnityEngine.UI;
using Zenject;

#pragma warning disable 0414

public class FPSCounter : MonoBehaviour, IFPSCounter
{
    private string extraMsg;

    public void AppendExtraInfo(string info)
    {
        info = "\n" + info;
        if (extraMsg == null)
            extraMsg = info;
        else
            extraMsg += info;
    }

    private Text text;
    private int frameInSec = 0;
    private float lastClearTime = -1;
    private int lastFPS = 0;

    void Awake()
    {
        text = GetComponent<Text>();
    }
    void Update()
    {
        if (Time.time - lastClearTime > 1)
        {
            lastFPS = frameInSec;
            frameInSec = 0;
            lastClearTime = Time.time;
        }

        string str = $"FPS : {lastFPS}{extraMsg ?? ""}";
        extraMsg = null;

        if (Time.timeScale == 0) return;

        frameInSec++;
        text.text = str;
    }
}
