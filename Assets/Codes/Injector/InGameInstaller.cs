using Assets.Codes.InGame.Input;
using BanGround;
using BanGround.Game.Mods;
using BanGround.Scene.Params;
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

    [Inject(Id = "r_notespeed")]
    private KVar r_notespeed;
    [Inject]
    private IFileSystem fs;

    public override void InstallBindings()
    {
        // Load parameters
        var parameters = SceneLoader.GetParamsOrDefault<InGameParams>();

        // Initiate mod manager
        var modManager = new ModManager(r_notespeed, parameters.mods);
        Container.Bind<IModManager>().FromInstance(modManager);

        // Bind all components
        Container.Unbind<IAudioTimelineSync>();
        Container.Bind<IAudioTimelineSync>().FromInstance(audioTimelineSync);
        Container.Bind<INoteController>().FromInstance(noteController);
        Container.Bind<IInGameBackground>().FromInstance(inGameBackground);
        Container.Bind<IUIManager>().FromInstance(uiManager);
        Container.Bind<IScript>().FromInstance(chartScript);

        var SM = new GameStateMachine();
        Container.Bind<IGameStateMachine>().FromInstance(SM);

        // Touch provider
        IKirakiraTouchProvider touchProvider;
        if (!string.IsNullOrEmpty(parameters.replayPath))
        {
            touchProvider = new DemoReplayTouchProvider(ProtobufHelper.Load<V2.ReplayFile>(fs.GetFile(parameters.replayPath)));
        }
        else if (parameters.mods.HasFlag(ModFlag.AutoPlay))
        {
            GameObject.Find("MouseCanvas").SetActive(false);
            touchProvider = new AutoPlayTouchProvider(SM);
        }
        else
        {
            /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        provider = new MultiMouseTouchProvider();
            #else*/
#if UNITY_EDITOR
            GameObject.Find("MouseCanvas").SetActive(false);
            touchProvider = new MouseTouchProvider();
#else
            GameObject.Find("MouseCanvas").SetActive(false);
            touchProvider = new InputSystemTouchProvider();
#endif
        }

        Container.Bind<IKirakiraTouchProvider>().FromInstance(touchProvider);
    }
}
