using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS;
using UnityEditor.iOS.Xcode;
using UnityEditor.EditorTools;
using System.IO;

public static class iOSBuilder
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string targetPath)
    {
        if (target != BuildTarget.iOS) return;

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
    }
}
