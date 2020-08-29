#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class iOSBuilder
{
    private static void UpdateInfo(string targetPath)
    {
        string path = Path.GetFullPath(targetPath);
        PlistDocument document = new PlistDocument();
        document.ReadFromFile(Path.Combine(path, "info.plist"));

        PlistElementDict plist = document.root;
        PlistElementDict CFBundleDocumentTypesDict;
        if (plist.values.ContainsKey("CFBundleDocumentTypes"))
        {
            CFBundleDocumentTypesDict = plist.values["CFBundleDocumentTypes"].AsArray().AddDict();
        }
        else
        {
            CFBundleDocumentTypesDict = plist.CreateArray("CFBundleDocumentTypes").AddDict();
        }
        CFBundleDocumentTypesDict.CreateArray("CFBundleTypeIconFiles");
        CFBundleDocumentTypesDict.SetString("CFBundleTypeName", "kirapack");
        CFBundleDocumentTypesDict.SetString("CFBundleTypeRole", "Editor");
        CFBundleDocumentTypesDict.SetString("LSHandlerRank", "Owner");
        CFBundleDocumentTypesDict.CreateArray("LSItemContentTypes").AddString("com.banground.pack");

        PlistElementDict UTExportedTypeDeclarationsDict = null;
        if (plist.values.ContainsKey("UTExportedTypeDeclarations"))
        {
            UTExportedTypeDeclarationsDict = plist.values["UTExportedTypeDeclarations"].AsArray().AddDict();
        }
        else
        {
            UTExportedTypeDeclarationsDict = plist.CreateArray("UTExportedTypeDeclarations").AddDict();
        }
        UTExportedTypeDeclarationsDict.CreateArray("UTTypeConformsTo").AddString("public.data");
        UTExportedTypeDeclarationsDict.SetString("UTTypeDescription", "kirapack");
        UTExportedTypeDeclarationsDict.CreateArray("UTTypeIconFiles");
        UTExportedTypeDeclarationsDict.SetString("UTTypeIdentifier", "com.banground.pack");
        UTExportedTypeDeclarationsDict.CreateDict("UTTypeTagSpecification").CreateArray("public.filename-extension").AddString("kirapack");

        plist.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);
        plist.SetBoolean("UIFileSharingEnabled", true);

        document.WriteToFile(Path.Combine(path, "info.plist"));
        Debug.Log("Write Plist Succeed!");
    }

    private static void UpdateProject(string targetPath)
    {
        string projectPath = PBXProject.GetPBXProjectPath(targetPath);

        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);
        string[] targetGuids = new string[2] { pbxProject.GetUnityMainTargetGuid(), pbxProject.GetUnityFrameworkTargetGuid() };

        pbxProject.SetBuildProperty(targetGuids, "ENABLE_BITCODE", "NO");
        pbxProject.WriteToFile(projectPath);

        Debug.Log("Write Project Succeed!");
    }

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string targetPath)
    {
        if (target != BuildTarget.iOS) return;

        OnPostCloudExport(targetPath);
    }

    public static void OnPostCloudExport(string targetPath)
    {
        UpdateInfo(targetPath);
        UpdateProject(targetPath);
    }
}
#endif