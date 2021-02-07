using AudioProvider;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;

    [SerializeField] 
    private TextAsset sound;
    private ISoundEffect se;
    private Button button;

    async void Start()
    {
        se = await audioManager.PrecacheSE(sound.bytes);

        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            se.PlayOneShot();
        });
    }
}
