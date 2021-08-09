using AudioProvider;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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

    private static Dictionary<TextAsset, ISoundEffect> soundEffectCache = new Dictionary<TextAsset, ISoundEffect>();

    private async UniTask<ISoundEffect> PrecacheOrGetSoundEffect(TextAsset ta)
    {
        if (soundEffectCache.ContainsKey(ta))
            return soundEffectCache[ta];

        var se = await audioManager.PrecacheSE(ta.bytes);
        soundEffectCache[ta] = se;

        return se;
    }

    async void Start()
    {
        se = await PrecacheOrGetSoundEffect(sound);

        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            se.PlayOneShot();
        });
    }
}
