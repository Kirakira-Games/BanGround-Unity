using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class MasterMemory : ScriptableObject
    {
        [MenuItem("BanGround/Generate Master Memory")]
        static void GenerateMasterMemory()
        {
            string path = Path.Combine(Application.dataPath, "..");
            path = Path.GetFullPath(path);
            var cmd = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = path + "/MasterMemory.Generator/win-x64/MasterMemory.Generator.exe",
                Arguments = $"-i \"{path}/Assets/Codes/Storage/Database/Models\" -o \"{path}/Assets/Codes/Storage/Database/Generated\" -n \"BanGround.Database.Models\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            cmd.StartInfo = startInfo;
            cmd.Start();
            string output = cmd.StandardOutput.ReadToEnd();
            string err = cmd.StandardError.ReadToEnd().Trim();
            if (err.Length > 0)
            {
                UnityEngine.Debug.Log(output);
                EditorUtility.DisplayDialog("Error", err, "Cancel");
            }
            else
            {
                EditorUtility.DisplayDialog("Done", output, "OK");
            }
            cmd.WaitForExit();
        }
    }
}