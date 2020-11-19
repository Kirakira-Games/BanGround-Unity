using UnityEngine;
using System.Collections;
using Zenject;
using AudioProvider;

public class AudioProviderFactory : IFactory<IAudioProvider>
{
    private DiContainer container;
    private string snd_engine;

    public AudioProviderFactory(DiContainer container, [Inject(Id = "snd_engine")] KVar snd_engine)
    {
        this.container = container;
        this.snd_engine = snd_engine;
    }

    public IAudioProvider Create()
    {
        switch (snd_engine)
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
