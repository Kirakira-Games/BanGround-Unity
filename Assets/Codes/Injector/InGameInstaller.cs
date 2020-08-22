using UnityEngine;
using Zenject;

public class InGameInstaller : MonoInstaller
{
    public AudioTimelineSync audioTimelineSync;
    public NoteController noteController;

    public override void InstallBindings()
    {
        Container.Unbind<IAudioTimelineSync>();
        Container.Bind<IAudioTimelineSync>().FromInstance(audioTimelineSync);
        Container.Bind<INoteController>().FromInstance(noteController);
    }
}
