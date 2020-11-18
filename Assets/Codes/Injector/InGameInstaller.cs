using BanGround.Scripting;
using BanGround.Scripting.Lunar;
using UnityEngine;
using Zenject;

public class InGameInstaller : MonoInstaller
{
    public AudioTimelineSync audioTimelineSync;
    public NoteController noteController;
    public InGameBackground inGameBackground;
    public UIManager uiManager;
    public LunarScript chartScript;

    public override void InstallBindings()
    {
        Container.Unbind<IAudioTimelineSync>();
        Container.Bind<IAudioTimelineSync>().FromInstance(audioTimelineSync);
        Container.Bind<INoteController>().FromInstance(noteController);
        Container.Bind<IInGameBackground>().FromInstance(inGameBackground);
        Container.Bind<IUIManager>().FromInstance(uiManager);
        Container.Bind<IScript>().FromInstance(chartScript);
        Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle().NonLazy();
    }
}
