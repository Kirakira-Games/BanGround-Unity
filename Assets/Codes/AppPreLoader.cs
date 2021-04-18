using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Zenject;
using Newtonsoft.Json;

public class AppPreLoader : MonoBehaviour
{
    [Inject]
    IKVSystem kvSystem;

    public static int sampleRate = -1;
    public static int bufferSize = -1;
    public static bool init = false;
    public static string UUID = string.Empty;

    private AndroidJavaObject s_ActivityContext = null;

    void Awake()
    {
        Physics.autoSimulation = false;
        Physics2D.simulationMode = SimulationMode2D.Script;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Screen.orientation = ScreenOrientation.AutoRotation;
        Application.targetFrameRate = 120;

        InitAudioInfo();
        GetUUID();
    }

    private void Start()
    {
        // Init Unitask
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopHelper.Initialize(ref playerLoop);

        // Init Json.Net
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    private void InitAudioInfo()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            sampleRate = 48000;
            bufferSize = 0;
            init = true;
            //SceneManager.LoadScene("Title");
        }
        else
        {
            AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (activityClass != null)
            {
                s_ActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
            AndroidJavaObject m_audio = new AndroidJavaObject("fun.banground.game.AudioInfo");
            m_audio.Call("Init", s_ActivityContext);

            string sr = m_audio.Call<string>("GetSampleRate");
            string bs = m_audio.Call<string>("GetBufferSize");

            bool success = false;
            success |= int.TryParse(sr, out sampleRate);
            success &= int.TryParse(bs, out bufferSize);
            init = success;

            //SceneManager.LoadScene("Title");
        }
    }

    private void GetUUID()
    {
        UUID = SystemInfo.deviceUniqueIdentifier;
    }
}
