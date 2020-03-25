using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppPreLoader : MonoBehaviour
{
    public static int sampleRate = -1;
    public static int bufferSize = -1;
    public static bool init = false;

    private AndroidJavaObject s_ActivityContext = null;

    void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        print(Screen.orientation);
        Application.targetFrameRate = 120;
        if (Application.platform != RuntimePlatform.Android) 
        {
            sampleRate = 48000;
            if (PlayerPrefs.GetString("AudioEngine", "Bass") == "Bass")
                bufferSize = 0;
            else
                bufferSize = 512;
            SceneManager.LoadScene("Title");
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

            //Debug.LogWarning($"Get Device Audio Info: {success}");
            //MessageBoxController.ShowMsg(LogLevel.INFO, "Load Device Audio Info Success");
            //MessageBoxController.ShowMsg(LogLevel.INFO, "SampleRate: " + sampleRate.ToString() + "  BufferSize: " + bufferSize.ToString());
            SceneManager.LoadScene("Title");
        }

        // Input system
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

//#if UNITY_IOS
        // File import
        Application.deepLinkActivated += (url) =>
        {
            //Debug.Log(url);
            if (DataLoader.LoadAllKiraPackFromInbox())
            {
                if (SceneManager.GetActiveScene().name == "Select")
                {
                    SceneManager.LoadScene("Select");
                }
            }
        };
//#endif
    }
}
