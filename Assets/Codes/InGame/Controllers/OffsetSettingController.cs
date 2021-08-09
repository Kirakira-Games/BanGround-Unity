using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OffsetSettingController : MonoBehaviour
{
    [Inject]
    private IUIManager UI;

    private Text offsetText;
    private const int RECENT_COUNT = 10;
    private Queue<int> recentQueue;
    private int sum;

    [Inject(Id = "r_notesize")]
    KVar r_notesize;
    [Inject(Id = "o_judge")]
    KVar o_judge;
    [Inject(Id = "o_audio")]
    KVar o_audio;

    private InputField noteSize;
    private InputField judgeOffset;
    private InputField audioOffset;

    void Start()
    {
        offsetText = GetComponent<Text>();
        recentQueue = new Queue<int>();
        sum = 0;
        offsetText.text = "+0";
        offsetText.color = Color.grey;

        //var noteSpeed = GameObject.Find("Speed_Input").GetComponent<InputField>();
        //noteSpeed.text = string.Format("{0:f1}", r_notespeed.Get<float>());
        noteSize = GameObject.Find("Size_Input").GetComponent<InputField>();
        noteSize.text = string.Format("{0:f1}", (float)r_notesize);
        judgeOffset = GameObject.Find("Judge_Input").GetComponent<InputField>();
        judgeOffset.text = o_judge;
        audioOffset = GameObject.Find("Audio_Input").GetComponent<InputField>();
        audioOffset.text = o_audio;
    }

    public void Add(int offset)
    {
        recentQueue.Enqueue(offset);
        sum += offset;
        while (recentQueue.Count > RECENT_COUNT)
            sum -= recentQueue.Dequeue();
        int value = Mathf.RoundToInt((float)sum / recentQueue.Count);
        offsetText.text = value >= 0 ? "+" + value : value.ToString();
        offsetText.color = value >= 0 ? Color.blue : Color.red;
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
        UI.GameRetire();
    }
}
