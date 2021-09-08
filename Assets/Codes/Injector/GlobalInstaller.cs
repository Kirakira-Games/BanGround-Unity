using AudioProvider;
using BanGround;
using BanGround.Community;
using BanGround.Database;
using BanGround.Identity;
using BanGround.Web;
using System.Linq;
using UnityEngine;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    public AccountManager accountManager;
    public FPSCounter fpsCounter;
    public LoadingBlocker loadingBlocker;
    public LocalizedStrings localizedStrings;
    public MessageBannerController messageBannerController;
    public MessageBox messageBox;
    public MessageCenter messageCenter;

    private IAudioProvider audioProvider;
    private IDataLoader dataLoader;
    private IFileSystem fs;
    private IKVSystem kvSystem;

    public override void InstallBindings()
    {
        // MasterMemory
        Initializer.SetupMessagePackResolver();

        // SceneLoader
        SceneLoader.Init();

        // Filesystem
        Container.Bind<IFileSystem>().To<LocalFilesystem>().AsSingle().OnInstantiated((_, obj) =>
        {
            if (obj is ValidationMarker) return;
            fs = obj as IFileSystem;
        });

        // Data Loader
        Container.Bind<IDataLoader>().To<DataLoader>().AsSingle().OnInstantiated((context, obj) =>
        {
            if (obj is ValidationMarker) return;
            dataLoader = obj as IDataLoader;
            dataLoader.InitFileSystem();
            dataLoader.Init();
            new GameObject("AppPreloader").AddComponent<AppPreLoader>();
        }).NonLazy();

        // Client-side database
        Container.Bind<IDatabaseAPI>().To<DatabaseAPI>().AsSingle().NonLazy();

        // KVar System
        Container.Bind<IKVSystem>().To<KVSystem>().AsSingle().OnInstantiated((_, obj) =>
        {
            if (obj is ValidationMarker) return;
            kvSystem = obj as IKVSystem;

            kvSystem.ReloadConfig();
        }).NonLazy();

        RegisterKonCommands();

        // Localized strings
        Container.Bind<LocalizedStrings>().FromInstance(localizedStrings).NonLazy();

        // Chart version
        Container.Bind<IChartVersion>().To<ChartVersion>().AsSingle().NonLazy();

        // Chart Loader
        Container.Bind<IChartLoader>().To<ChartLoader>().AsSingle().NonLazy();

        // Audio Manager
        Container.Bind<IAudioProvider>().FromFactory<AudioProviderFactory>().AsSingle().NonLazy();
        Container.Bind<IAudioManager>().To<AudioManager>().FromNewComponentOnNewGameObject().AsSingle().OnInstantiated((_, obj) =>
        {
            if (obj is ValidationMarker) return;
            audioProvider = (obj as IAudioManager).Provider;
        }).NonLazy();

        // Sorter Factory
        Container.Bind<ISorterFactory>().To<SorterFactory>().AsSingle().NonLazy();

        // Chart List Manager
        Container.Bind<IChartListManager>().To<ChartListManager>().AsSingle().NonLazy();

        // Resource Loader
        Container.Bind<IResourceLoader>().To<ResourceLoader>().AsSingle().NonLazy();

        // Message Canvas
        Container.Bind<IMessageBox>().FromInstance(messageBox);
        Container.Bind<IMessageBannerController>().FromInstance(messageBannerController);

        // Cancellation Token Store
        Container.Bind<ICancellationTokenStore>().To<CancellationTokenStore>().AsSingle().NonLazy();

        // Kira Web Request
        Container.Bind<IKiraWebRequest>().To<KiraWebRequest>().AsSingle().NonLazy();

        // FPS Counter
        Container.Bind<IFPSCounter>().FromInstance(fpsCounter);

        // Loading blocker
        Container.Bind<ILoadingBlocker>().FromInstance(loadingBlocker);

        // Message center
        Container.Bind<IMessageCenter>().FromInstance(messageCenter);

        // Download manager
        Container.Bind<IDownloadManager>().To<DownloadManager>().AsSingle().NonLazy();

        // Account manager
        Container.Bind<IAccountManager>().FromInstance(accountManager);

        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
    }

    void RegisterKonCommands()
    {
        KVar.KVarInfo[] varInfos =
        {
            KVar.C("o_judge", "0", KVarFlags.Archive, "Judge offset"),
            KVar.C("o_audio", "0", KVarFlags.Archive, "Audio offset"),

            KVar.C("r_notespeed", "10.0", KVarFlags.Archive, "Note speed for rendering"),
            KVar.C("r_notesize", "1.0", KVarFlags.Archive, "Note size for rendering"),
            KVar.C("r_syncline", "1", KVarFlags.Archive, "Show syncline"),
            KVar.C("r_lanefx", "1", KVarFlags.Archive, "Show lane effects while clicking on lanes"),
            KVar.C("r_graynote", "1", KVarFlags.Archive, "Enables the \"Off-beat coloring\" aka grey notes"),
            KVar.C("r_bang_perspect", "1", KVarFlags.Archive, "Use BanG Dream style perspect instead of real 3d perspect"),
            KVar.C("r_tap_effect", "1", KVarFlags.Archive, "Play special effect on note hit"),

            KVar.C("r_shake_flick", "1", KVarFlags.Archive, "Shake the screen while flicker note judged"),
            KVar.C("r_usevideo", "1", KVarFlags.Archive, "Use video for background (If present)"),
            KVar.C("r_farclip", "162.0", KVarFlags.Archive, "Far clip of note camera (means length of lane)"),

            KVar.C("r_brightness_bg", "0.7", KVarFlags.Archive, "Background brightness"),
            KVar.C("r_brightness_lane", "0.84", KVarFlags.Archive, "Lane brightness"),
            KVar.C("r_brightness_long", "0.8", KVarFlags.Archive, "Brightness of Longs or Slides"),

            KVar.C("r_lowresolution", "0", KVarFlags.Archive, "Low Resolution(0.7x)"),

#if UNITY_STANDALONE_WIN
            KVar.C("win_width", "1280", KVarFlags.Archive, "Window width"),
            KVar.C("win_height", "720", KVarFlags.Archive, "Window height"),

            KVar.C("r_fullscreen", "1", KVarFlags.Archive, "Fullscreen (windows only)", (_, kvSystem) =>
            {
#if !UNITY_EDITOR
                var r_fullscreen = kvSystem.Find("r_fullscreen");
                var win_width = kvSystem.Find("win_width");
                var win_height = kvSystem.Find("win_height");

                if(r_fullscreen)
                {
                    var r = Screen.resolutions[Screen.resolutions.Length - 1];
                    Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow);
                    Screen.fullScreen = true;
                }
                else
                {
                    Screen.SetResolution(win_width, win_height, FullScreenMode.Windowed);
                }
#endif
            }),

            KVar.C("r_vsync", "0", KVarFlags.Archive, "Enable VSync", (_, kvSystem) =>
            {
#if !UNITY_EDITOR
                var r_vsync = kvSystem.Find("r_vsync");

                QualitySettings.vSyncCount = r_vsync ? 1 : 0;
#endif
            }),
#endif

                KVar.C("cl_showms", "0", KVarFlags.Archive),
            KVar.C("cl_elp", "0", KVarFlags.Archive),
            KVar.C("cl_offset_transform", "1", KVarFlags.Archive),

            KVar.C("cl_notestyle", "0", KVarFlags.Archive),
            KVar.C("cl_sestyle", "1", KVarFlags.Archive),

            KVar.C("fs_assetpath", "V2Assets", KVarFlags.Hidden | KVarFlags.StringOnly),
            KVar.C("fs_iconpath", "UI/ClearMark", KVarFlags.Hidden | KVarFlags.StringOnly),

            KVar.C("cl_lastdiff", "0", KVarFlags.Archive, "Current chart set difficulty", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_cursorter", "1", KVarFlags.Archive, "Current sorter type", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_lastsid", "-1", KVarFlags.Archive, "Current chart set id", (_, kvSystem) => kvSystem.SaveConfig()),

            KVar.C("cl_accesstoken", "", KVarFlags.Archive | KVarFlags.StringOnly, "Saved access token", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_refreshtoken", "", KVarFlags.Archive | KVarFlags.StringOnly, "Saved refresh token", (_, kvSystem) => kvSystem.SaveConfig()),

            KVar.C("snd_bgm_volume", "0.7", KVarFlags.Archive, "BGM volume", _ =>
            {
                audioProvider?.SetSoundTrackVolume(kvSystem.Find("snd_bgm_volume"));
            }),
            KVar.C("snd_se_volume", "0.7", KVarFlags.Archive, "Sound effect volume",_ =>
            {
                audioProvider?.SetSoundEffectVolume(kvSystem.Find("snd_se_volume"), SEType.Common);
            }),
            KVar.C("snd_igse_volume", "0.7", KVarFlags.Archive, "In-game sound effect volume",_ =>
            {
                audioProvider?.SetSoundEffectVolume(kvSystem.Find("snd_igse_volume"), SEType.InGame);
            }),

            KVar.C("snd_engine", "Fmod", KVarFlags.Archive | KVarFlags.StringOnly, "Sound engine type"),
            KVar.C("snd_buffer_bass", "-1", KVarFlags.Archive, "Buffer type of Bass Sound Engine"),
            KVar.C("snd_buffer_fmod", "-1", KVarFlags.Archive, "Buffer size of Fmod/Unity Sound Engine"),

            KVar.C("g_saveReplay", "1", KVarFlags.Archive, "Enables replay recording."),

            KVar.C("snd_output", ((int)FMOD.OUTPUTTYPE.AUTODETECT).ToString(), KVarFlags.Archive),

            KVar.C("cl_language", "-1", KVarFlags.Archive),

            KVar.C("cl_modflag", "0", KVarFlags.StringOnly, "A hex string storing ModFlag. At most 64 bits."),
            KVar.C("cl_mappingmodflag", "0", KVarFlags.StringOnly,
            "A hex string storing ModFlag specifically for mapping. At most 64 bits."),

            KVar.C("skin_particle", "meigong", KVarFlags.Archive | KVarFlags.StringOnly, "Particle name"),

            KVar.C("rm_data_version", "-1", KVarFlags.Hidden),
            KVar.C("rm_ver_stable", Application.version, KVarFlags.Hidden),
            KVar.C("rm_ver_min", Application.version, KVarFlags.Hidden),
        };

        foreach (var info in varInfos)
            Container.Bind<KVar>().WithId(info.Name).AsCached().OnInstantiated(KVar.OnInit(info)).NonLazy();

        Kommand.KommandInfo[] cmdInfos =
        {
            Kommand.C("move_chart", "Move a chart", args =>
            {
                if(args.Length != 2)
                    return;

                int chartA = int.Parse(args[0]);
                int targetChart = int.Parse(args[1]);

                dataLoader.MoveChart(chartA, targetChart);
            }),
            Kommand.C("savecfg", "Save configs", _ => kvSystem.SaveConfig()),
            Kommand.C("exec", "Execute a config file", (string[] args) =>
            {
                if (args == null || args.Length == 0)
                    Debug.Log("Useage: exec <cfg file name>");

                var filename = args[0];
                if (!filename.EndsWith(".cfg"))
                    filename += ".cfg";

                kvSystem.ExecuteFile(filename);
            }),
            Kommand.C("echo", "Repeater", (string[] args) =>
            {
                var str = "";
                args.All(arg =>
                {
                    str += arg + " ";
                    return true;
                });

                Debug.Log(str);
            }),
            Kommand.C("help", "List available kommands and kvars", () =>
            {
                var table = "<table cellspacing=\"5\"><tr><td>Name</td><td>Type</td><td>Description</td></tr>\n" +
                            "<tr><td height=\"1\" colspan=\"3\" style=\"background-color:#0c0;\"></td></tr>";

                foreach (var cmd in kvSystem)
                {
                    bool show = true;
                    string type = "Kommand";

                    if(cmd is KVar kVar)
                    {
                        if (kVar.IsFlagSet(KVarFlags.Hidden)
#if !DEBUG
                            || kVar.IsFlagSet(KVarFlags.DevelopmentOnly)
#endif
                        ) show = false;

                        type = "KVar";
                    }

                    if (show)
                        table += $"<tr><td>{cmd.Name}</td><td>{type}</td><td>{cmd.Description}</td></tr>";
                }

                table += "</table>";
                Debug.Log(table);
            }),
        };

        foreach (var info in cmdInfos)
            Container.Bind<Kommand>().WithId(info.Name).AsCached().OnInstantiated(Kommand.OnInit(info)).NonLazy();
    }

    int fsUpdateFrameCounter = 0;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    int lastRes = -1;
    KVar r_fullscreen;
    KVar win_width;
    KVar win_height;
#endif

    private void Update()
    {
        if (++fsUpdateFrameCounter > 3600)
        {
            fsUpdateFrameCounter = 0;
            //fs.OnUpdate();
        }

#if UNITY_STANDALONE_WIN  && !UNITY_EDITOR
        if(lastRes == -1)
        {
            r_fullscreen = kvSystem.Find("r_fullscreen");
            win_width = kvSystem.Find("win_width");
            win_height = kvSystem.Find("win_height");

            lastRes = Screen.width * Screen.height;
        }
        else if(!r_fullscreen)
        {
            var curRes = Screen.width * Screen.height;

            if(curRes != lastRes)
            {
                lastRes = curRes;
                win_width.Set(Screen.width);
                win_height.Set(Screen.height);
            }
        }
#endif
    }

    private void OnDestroy()
    {
        fs.Shutdown();
        kvSystem.SaveConfig();
    }
}
