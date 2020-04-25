using UnityEngine;
using UnityEditor;
 
public class HackWindowsIL2CPP
{
    public static void PreExport()
    {
        string status = "Default";
 
#if UNITY_CLOUD_BUILD
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        status = "Customized To Use Mono";
#endif
        Debug.Log($"Build Configuration - {status}");
    }
}