#if UNITY_ANDROID
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AndroidCallback : AndroidJavaProxy
{
    public AndroidCallback() : base("fun.banground.game.FileImportCallback") { }
    
    public void onFileImport()
    {
        if (DataLoader.LoadAllKiraPackFromInbox())
        {
            if (SceneManager.GetActiveScene().name == "Select")
            {
                SceneManager.LoadScene("Select");
            }
        }
    }
}
#endif