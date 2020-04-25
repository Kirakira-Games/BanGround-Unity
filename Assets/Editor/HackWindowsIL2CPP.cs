using UnityEngine;
using UnityEditor;
 
public class HackWindowsIL2CPP
{
    public static void PreExport()
    {
        string status = "Default";

#if UNITY_CLOUD_BUILD && UNITY_STANDALONE_WIN
        UnityEditor.PlayerSettings.SetScriptingBackend(UnityEditor.BuildTargetGroup.Standalone, UnityEditor.ScriptingImplementation.Mono2x);
        status = "Customized To Use Mono";
#endif
        Debug.Log($"Build Configuration - {status}");
    }
}