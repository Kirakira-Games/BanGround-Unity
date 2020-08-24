using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OffsetSettingController : MonoBehaviour
{
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IModManager modManager;
    [Inject]
    private IUIManager UI;

    private Text offsetText;
    private const int RECENT_COUNT = 10;
    private Queue<int> recentQueue;
    private int sum;

    private static KVarRef r_notesize = new KVarRef("r_notesize");
    private static KVarRef o_judge = new KVarRef("o_judge");
    private static KVarRef o_audio = new KVarRef("o_audio");

    private InputField noteSize;
    private InputField judgeOffset;
    private InputField audioOffset;

    void Start()
    {
        Debug.Log("Start");
        offsetText = GetComponent<Text>();
        recentQueue = new Queue<int>();
        sum = 0;
        offsetText.text = "+0";
        offsetText.color = Color.blue;

        //var noteSpeed = GameObject.Find("Speed_Input").GetComponent<InputField>();
        //noteSpeed.text = string.Format("{0:f1}", r_notespeed.Get<float>());
        noteSize = GameObject.Find("Size_Input").GetComponent<InputField>();
        noteSize.text = string.Format("{0:f1}", r_notesize.Get<float>());
        judgeOffset = GameObject.Find("Judge_Input").GetComponent<InputField>();
        judgeOffset.text = o_judge.Get<int>().ToString();
        audioOffset = GameObject.Find("Audio_Input").GetComponent<InputField>();
        audioOffset.text = o_audio.Get<int>().ToString();
    }

    public void Add(int offset)
    {
        recentQueue.Enqueue(offset);
        sum += offset;
        while (recentQueue.Count > RECENT_COUNT)
            sum -= recentQueue.Dequeue();
        int value = Mathf.RoundToInt((float)sum / recentQueue.Count);
        offsetText.text = value >= 0 ? "+" + value : value.ToString();
        offsetText.color = value >= 0 ? Color.cyan : Color.red;
    }

    public void UpdateValue()
    {
        if (audioOffset == null)
            return; // Has not finished initialization
        r_notesize.Set(float.Parse(noteSize.text));
        o_judge.Set(int.Parse(judgeOffset.text));
        o_audio.Set(int.Parse(audioOffset.text));
    }

    public void SaveAndExit()
    {
        modManager.SuppressAllMods(false);
        chartListManager.ClearForcedChart();
        UI.GameRetire();
    }
}
