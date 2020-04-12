using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AudioProvider;

public class SESelector : MonoBehaviour
{
    private InputField seInput;

    private Button perfectBtn;
    private Button flickBtn;
    private ISoundEffect perfectSE;
    private ISoundEffect flickSE;

    private void Awake()
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
        seInput.onValueChanged.AddListener(v =>
        {
            if (int.Parse(seInput.text) < 1) seInput.text = "2";
            if (int.Parse(seInput.text) > 2) seInput.text = "1";
            perfectSE?.Dispose();
            flickSE?.Dispose();
            perfectSE = AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)int.Parse(seInput.text)) + "/perfect.wav").bytes);
            flickSE = AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)int.Parse(seInput.text)) + "/flick.wav").bytes);
        });

        perfectSE = AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) + "/perfect.wav").bytes);
        flickSE = AudioManager.Instance.PrecacheSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) + "/flick.wav").bytes);

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