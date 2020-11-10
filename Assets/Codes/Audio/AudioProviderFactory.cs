using UnityEngine;
using System.Collections;
using Zenject;
using AudioProvider;

public class AudioProviderFactory : IFactory<IAudioProvider>
{
    private DiContainer container;

    [Inject(Id = "snd_engine")]
    private KVar snd_engine;

    public AudioProviderFactory(DiContainer container)
    {
        this.container = container;
    }

    public IAudioProvider Create()
    {
        switch ((string)snd_engine)
        {
            case "Bass":
                return container.Instantiate<BassAudioProvider>();
            case "Fmod":
                return container.Instantiate<FmodAudioProvider>();
            /*case "Unity":
                return container.Instantiate<PureUnityAudioProvider>();*/
            default:
                Debug.LogError("Cannot recognize sound engine: " + snd_engine);
                return null;
        }
    }
}
