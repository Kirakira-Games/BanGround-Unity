#if UNITY_ANDROID && false
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AndroidCallback : AndroidJavaProxy
{
    public AndroidCallback() : base("fun.banground.game.FileImportCallback") { }

    public static AndroidCallback instance;

    public static void Init()
    {
        instance = new AndroidCallback();
        AndroidJavaClass pluginClass = new AndroidJavaClass("fun.banground.game.KirakiraActivity");
        pluginClass.CallStatic("registerFileImportCallback", instance);
    }

    public void onFileImport()
    {
        //if (dataLoader.LoadAllKiraPackFromInbox())
        //{
        //    if (SceneManager.GetActiveScene().name == "Select")
        //    {
        //        SceneManager.LoadScene("Select");
        //    }
        //}
    }
}
#endif