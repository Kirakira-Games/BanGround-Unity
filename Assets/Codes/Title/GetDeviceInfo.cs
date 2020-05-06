using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class GetDeviceInfo : MonoBehaviour
{
    [SerializeField] private Button openBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Text infoText;
    [SerializeField] private Animator animator;

    //private void Awake()
    //{
    //    openBtn = transform.Find("InfoBtn").GetComponent<Button>();
    //    closeBtn = transform.Find("Wnd").GetComponent<Button>();
    //    infoText = transform.Find("InfoText").GetComponent<Text>();
    //    animator = GetComponent<Animator>();
    //}

    void Start()
    {

        openBtn.onClick.AddListener(() =>
        {
            animator.Play("FlyIn");
        });

        closeBtn.onClick.AddListener(() =>
        {
            animator.Play("FlyOut");
        });

        KVarRef snd_engine = new KVarRef("snd_engine");

        StringBuilder sb = new StringBuilder();
        sb.Append("Device Model: ").AppendLine(SystemInfo.deviceModel).AppendLine();
        sb.Append("System: ").AppendLine(SystemInfo.operatingSystem).AppendLine();
        sb.Append("CPU: ").AppendLine(SystemInfo.processorType).AppendLine();
        sb.Append("GPU: ").AppendLine(SystemInfo.graphicsDeviceName).AppendLine();
        sb.Append("AudioProvider: ").AppendLine(snd_engine).AppendLine();
#if UNITY_ANDROID && !UNITY_EDITOR
        sb.Append("SampleRate: ").AppendLine(AppPreLoader.sampleRate.ToString()).AppendLine();
        sb.Append("BufferSize: ").AppendLine(AppPreLoader.bufferSize.ToString()).AppendLine();
        sb.Append("CurrentBuf: ").AppendLine(GetCurrentBuffer()).AppendLine();
#endif

        infoText.text = sb.ToString();
    }

    private string GetCurrentBuffer()
    {
        KVarRef snd_engine = new KVarRef("snd_engine");
        KVarRef snd_buffer_bass = new KVarRef("snd_buffer_bass");
        KVarRef snd_buffer_fmod = new KVarRef("snd_buffer_fmod");

        int bufferIndex;
        if (snd_engine == "Bass")
        {
            bufferIndex = snd_buffer_bass;
            return (AppPreLoader.bufferSize * HandelValue_Buffer.BassBufferScale[bufferIndex]).ToString();
        }
        else
        {
            bufferIndex = snd_buffer_fmod;
            return (AppPreLoader.bufferSize / HandelValue_Buffer.FmodBufferScale[bufferIndex]).ToString();
        }

    }
}
