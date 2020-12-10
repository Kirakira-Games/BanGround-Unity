using Assets.Codes.InGame.Input;
using BanGround;
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

    [Inject(Id = "g_demoRecord")]
    private KVar g_demoRecord;
    [Inject(Id = "cl_currentdemo")]
    private KVar cl_currentdemo;
    [Inject]
    private IModManager modManager;
    [Inject]
    private IFileSystem fs;

    public override void InstallBindings()
    {
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
        if (cl_currentdemo != "")
        {
            touchProvider = new DemoReplayTouchPrivider(DemoFile.LoadFrom(fs.GetFile(cl_currentdemo)));
            g_demoRecord.Set(false);
        }
        else if (modManager.isAutoplay)
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
            touchProvider = new InputManagerTouchProvider();
#endif
        }

        Container.Bind<IKirakiraTouchProvider>().FromInstance(touchProvider);
    }
}
