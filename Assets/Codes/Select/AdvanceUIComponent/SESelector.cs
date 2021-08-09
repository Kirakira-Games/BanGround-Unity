using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AudioProvider;
using Zenject;

public class SESelector : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IResourceLoader resourceLoader;

    private InputField seInput;

    private Button perfectBtn;
    private Button flickBtn;
    private ISoundEffect perfectSE;
    private ISoundEffect flickSE;

    private async void Awake()
    {
        seInput = GameObject.Find("SE_Input").GetComponent<InputField>();
        perfectBtn = GameObject.Find("SE_P_Player").GetComponent<Button>();
        flickBtn = GameObject.Find("SE_F_Player").GetComponent<Button>();
        GameObject.Find("SE<").GetComponent<Button>().onClick.AddListener(() =>
        {
            seInput.text = (int.Parse(seInput.text) - 1).ToString();
        });
        GameObject.Find("SE>").GetComponent<Button>().onClick.AddListener(() =>
        {
            seInput.text = (int.Parse(seInput.text) + 1).ToString();
        });
        seInput.onValueChanged.AddListener(async v =>
        {
            if (int.Parse(seInput.text) < 1) seInput.text = "3";
            if (int.Parse(seInput.text) > 3) seInput.text = "1";
            perfectSE?.Dispose();
            flickSE?.Dispose();
            perfectSE = await audioManager.PrecacheSE(resourceLoader.LoadSEResource<TextAsset>("perfect.wav", GetSE()).bytes);
            flickSE = await audioManager.PrecacheSE(resourceLoader.LoadSEResource<TextAsset>("flick.wav", GetSE()).bytes);
        });

        perfectSE = await audioManager.PrecacheSE(resourceLoader.LoadSEResource<TextAsset>("perfect.wav").bytes);
        flickSE = await audioManager.PrecacheSE(resourceLoader.LoadSEResource<TextAsset>("flick.wav").bytes);

        perfectBtn.onClick.AddListener(() =>
        {
            perfectSE?.PlayOneShot();
        });
        flickBtn.onClick.AddListener(() =>
        {
            flickSE?.PlayOneShot();
        });
    }

    public SEStyle GetSE()
    {
        return (SEStyle)int.Parse(seInput.text);
    }

    public void SetSE(SEStyle se)
    {
        seInput.text = ((int)se).ToString();
    }
}
