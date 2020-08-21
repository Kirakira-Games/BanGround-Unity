using UnityEngine;
using Zenject;

public class InGameInstaller : MonoInstaller
{
    public AudioTimelineSync audioTimelineSync;

    public override void InstallBindings()
    {
        Container.Unbind<IAudioTimelineSync>();
        Container.Bind<IAudioTimelineSync>().FromInstance(audioTimelineSync);
    }
}
