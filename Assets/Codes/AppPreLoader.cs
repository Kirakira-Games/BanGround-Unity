using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using UniRx.Async;

public class AppPreLoader : MonoBehaviour
{
    public static int sampleRate = -1;
    public static int bufferSize = -1;
    public static bool init = false;
    public static string UUID = string.Empty;

    private AndroidJavaObject s_ActivityContext = null;

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DataLoader.InitFileSystem();
        KVSystem.Instance.ReloadConfig();

        Screen.orientation = ScreenOrientation.AutoRotation;
        Application.targetFrameRate = 120;
        InitAudioInfo();
        GetUUID();

        Application.deepLinkActivated += (url) =>
        {
            if (DataLoader.LoadAllKiraPackFromInbox())
            {
                if (SceneManager.GetActiveScene().name == "Select")
                {
                    SceneManager.LoadScene("Select");
                }
            }
        };

        // Init Unitask
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopHelper.Initialize(ref playerLoop);

        SceneManager.LoadScene("Title");
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
#if UNITY_IOS && !UNITY_EDITOR
        UUID = UUIDToolExtern.getUUIDInKeychain();
#else
        UUID = SystemInfo.deviceUniqueIdentifier;
#endif
    }
}
public class UUIDToolExtern
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _getUUIDInKeychain();
    public static string getUUIDInKeychain()
    {
        return _getUUIDInKeychain();
    }
#endif
}
