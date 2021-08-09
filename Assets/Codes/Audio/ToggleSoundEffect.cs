using AudioProvider;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Toggle))]
public class ToggleSoundEffect : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;

    [SerializeField] 
    private TextAsset soundEnable;
    private TextAsset soundDisable;
    private ISoundEffect see;
    private ISoundEffect sed;
    private Toggle toggle;

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
        see = await PrecacheOrGetSoundEffect(soundEnable);
        sed = await PrecacheOrGetSoundEffect(soundDisable);

        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(v => (v ? see : sed).PlayOneShot());
    }
}
