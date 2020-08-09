using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    private IKVSystem kvSystem;
    private IDataLoader dataLoader;

    public override void Start()
    {
        kvSystem.ReloadConfig();
    }

    public override void InstallBindings()
    {
        // Data Loader
        Container.Bind<IDataLoader>().To<DataLoader>().AsSingle().OnInstantiated((context, obj) =>
        {
            if (obj is ValidationMarker) return;
            dataLoader = obj as IDataLoader;
            dataLoader.InitFileSystem();
            DataLoader.Instance = dataLoader; // TODO: Remove
            new GameObject("AppPreloader").AddComponent<AppPreLoader>();
        }).NonLazy();

        // KVar System
        Container.Bind<IKVSystem>().To<KVSystem>().AsSingle().OnInstantiated((_, obj) =>
        {
            kvSystem = obj as IKVSystem;
        }).NonLazy();

        RegisterKonCommands();

        // Chart version
        Container.Bind<IChartVersion>().To<ChartVersion>().AsSingle().OnInstantiated((contxet, obj) =>
        {
            if (obj is ValidationMarker) return;
            ChartVersion.Instance = obj as IChartVersion; // TODO: Remove
        }).NonLazy();
        // Audio Manager
        Container.Bind<IAudioManager>().To<AudioManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
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
            KVar.C("r_mirror", "0", KVarFlags.Archive, "Mirror the chart"),
            KVar.C("r_bang_perspect", "1", KVarFlags.Archive, "Use BanG Dream style perspect instead of real 3d perspect"),

            KVar.C("r_shake_flick", "1", KVarFlags.Archive, "Shake the screen while flicker note judged"),
            KVar.C("r_usevideo", "1", KVarFlags.Archive, "Use video for background (If present)"),
            KVar.C("r_farclip", "196.0", KVarFlags.Archive, "Far clip of note camera (means length of lane)"),

            KVar.C("r_brightness_bg", "0.7", KVarFlags.Archive, "Background brightness"),
            KVar.C("r_brightness_lane", "0.84", KVarFlags.Archive, "Lane brightness"),
            KVar.C("r_brightness_long", "0.8", KVarFlags.Archive, "Brightness of Longs or Slides"),

            KVar.C("r_showeffect", "1", KVarFlags.Archive, "Show LaneParticleEffect"),
            KVar.C("r_lowresolution", "0", KVarFlags.Archive, "Low Resolution(0.7x)"),

            KVar.C("cl_showms", "0", KVarFlags.Archive),
            KVar.C("cl_elp", "0", KVarFlags.Archive),
            KVar.C("cl_offset_transform", "1", KVarFlags.Archive),

            KVar.C("cl_notestyle", "0", KVarFlags.Archive),
            KVar.C("cl_sestyle", "1", KVarFlags.Archive),

            KVar.C("mod_autoplay", "0"),

            KVar.C("fs_assetpath", "V2Assets", KVarFlags.Hidden | KVarFlags.StringOnly),
            KVar.C("fs_iconpath", "UI/ClearMark/", KVarFlags.Hidden | KVarFlags.StringOnly),

            KVar.C("cl_lastdiff", "0", KVarFlags.Archive, "Current chart set difficulty", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_cursorter", "1", KVarFlags.Archive, "Current sorter type", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_lastsid", "-1", KVarFlags.Archive, "Current chart set id", (_, kvSystem) => kvSystem.SaveConfig()),

            KVar.C("cl_accesstoken", "", KVarFlags.Archive | KVarFlags.StringOnly, "Saved access token", (_, kvSystem) => kvSystem.SaveConfig()),
            KVar.C("cl_refreshtoken", "", KVarFlags.Archive | KVarFlags.StringOnly, "Saved refresh token", (_, kvSystem) => kvSystem.SaveConfig()),

            KVar.C("snd_bgm_volume", "0.7", KVarFlags.Archive, "BGM volume"),
            KVar.C("snd_se_volume", "0.7", KVarFlags.Archive, "Sound effect volume"),
            KVar.C("snd_igse_volume", "0.7", KVarFlags.Archive, "In-game sound effect volume"),

            KVar.C("snd_engine", "Fmod", KVarFlags.Archive | KVarFlags.StringOnly, "Sound engine type"),
            KVar.C("snd_buffer_bass", "-1", KVarFlags.Archive, "Buffer type of Bass Sound Engine"),
            KVar.C("snd_buffer_fmod", "-1", KVarFlags.Archive, "Buffer size of Fmod/Unity Sound Engine"),

            KVar.C("g_demoRecord", "1", KVarFlags.Archive, "Enables demo recording."),

            KVar.C("snd_output", ((int)FMOD.OUTPUTTYPE.AUTODETECT).ToString(), KVarFlags.Archive),

            KVar.C("cl_language", "-1", KVarFlags.Archive),
        };

        foreach (var info in varInfos)
            Container.Bind<KVar>().WithId(info.Name).AsCached().OnInstantiated(KVar.OnInit(info)).NonLazy();

        Kommand.KommandInfo[] cmdInfos =
        {
            Kommand.C("savecfg", "Save configs", _ => kvSystem.SaveConfig()),
            Kommand.C("exec", "Execute a config file", (string[] args) =>
            {
                if (args == null || args.Length == 0)
                    Debug.Log("Useage: exec <cfg file name>");

                var filename = args[0];
                if (!filename.EndsWith(".cfg"))
                    filename += ".cfg";

                if (KiraFilesystem.Instance.Exists(filename))
                {
                    string[] cfg = KiraFilesystem.Instance.ReadString(filename).Replace("\r", "").Split('\n');

                    cfg.All(line => {
                        kvSystem.ExecuteLine(line, true);
                        return true;
                    });
                }
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
                        table += $"<tr><td>{name}</td><td>{type}</td><td>{cmd.Description}</td></tr>";
                }
    
                table += "</table>";
                Debug.Log(table);
            }),
            Kommand.C("demo_play", "Play a demo file", args =>
            {
                if (args.Length > 0)
                {
                    if (SceneManager.GetActiveScene().name == "Select")
                    {
                        var path = args[0];

                        if (!File.Exists(path))
                        {
                            if (KiraFilesystem.Instance.Exists(path))
                            {
                                path = Path.Combine(DataLoader.DataDir, path);
                            }
                            else
                            {
                                Debug.Log("[Demo Player] File not exists");
                                return;
                            }
                        }

                        var file = DemoFile.LoadFrom(path);

                        var targetHeader = dataLoader.chartList.First(x => x.sid == file.sid);

                        if (targetHeader == null)
                        {
                            Debug.Log("[Demo Player] Target chartset not installed.");
                            return;
                        }

                        if (targetHeader.difficultyLevel[(int)file.difficulty] == -1)
                        {
                            Debug.Log("[Demo Player] Target chart not installed.");
                            return;
                        }

                        LiveSetting.currentChart = dataLoader.chartList.IndexOf(dataLoader.chartList.First(x => x.sid == file.sid));
                        LiveSetting.actualDifficulty = (int)file.difficulty;
                        LiveSetting.currentDifficulty.Set((int)file.difficulty);

                        LiveSetting.DemoFile = file;

                        SceneLoader.LoadScene("Select", "InGame", () => LiveSetting.LoadChart(true));
                    }
                    else
                    {
                        Debug.Log("[Demo Player] Must use in select page!");
                    }
                }
                else
                {
                    Debug.Log("demo_play: Play a demo file<br />Usage: demo_play <demo file>");
                }
            }),
        };

        foreach (var info in cmdInfos)
            Container.Bind<Kommand>().WithId(info.Name).AsCached().OnInstantiated(Kommand.OnInit(info)).NonLazy();
    }
}