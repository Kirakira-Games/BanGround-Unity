using System;
using System.Collections.Generic;
using System.Text;
using BanGround;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

public class LocalizedStrings : MonoBehaviour
{
    [Inject]
    IFileSystem fs;

    public TextAsset[] languageFiles = new TextAsset[25];
    private Dictionary<string, string> dictionary = null;
    private Dictionary<string, string> fallbackDictionary = null;

    [Inject(Id = "cl_language")]
    KVar cl_language;

    private const int fallbackLanguage = 0;

    internal static LocalizedStrings instance;

    [Inject]
    public void Inject()
    {
        if(cl_language == -1)
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    cl_language.Set((int)Language.SimplifiedChinese);
                    break;
                /*
                case SystemLanguage.ChineseTraditional:
                    cl_language.Set((int)Language.TraditionalChinese);
                    break;
                case SystemLanguage.Japanese:
                    cl_language.Set((int)Language.Japanese);
                    break;
                case SystemLanguage.Korean:
                    cl_language.Set((int)Language.Korean);
                    break;
                */
                default:
                    cl_language.Set((int)Language.English);
                    break;
            }
        }

        ReloadLanguageFile(cl_language);
        instance = this;
    }

    public string GetLocalizedString(string str)
    {
        if (dictionary == null)
        {
            Debug.LogError("[LocalizedStrings] Dictionary not initialized yet!");
            return str;
        }
        if (dictionary.ContainsKey(str))
            return dictionary[str];

        Debug.LogWarning($"Missing localized entry: {str}");

        if (fallbackDictionary == null)
        {
            Debug.LogError("[LocalizedStrings] Fallback Dictionary not initialized yet!");
            return str;
        }

        if (fallbackDictionary.ContainsKey(str))
            return fallbackDictionary[str];

        Debug.LogWarning($"Missing localized entry in fallback language (english): {str}");

        return str;
    }

    public void ReloadLanguageFile(int language)
    {
        dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFiles[language].text);
        fallbackDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFiles[fallbackLanguage].text);
    }
}

public static class StringExt
{
    /// <summary>
    /// Convert this string to Localized string
    /// </summary>
    /// <param name="str">reference string</param>
    /// <returns>Localized string</returns>
    public static string L(this string str)
    {
        return LocalizedStrings.instance.GetLocalizedString(str);
    }

    /// <summary>
    /// Convert this string to Localized string and format it
    /// </summary>
    /// <param name="str">reference format string</param>
    /// <param name="args">objects to format</param>
    /// <returns></returns>
    public static string L(this string str, params object[] args)
    {
        string localized = str.L();

        try
        {
            return string.Format(localized, args);
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"{str} is a formatted string, but there's issue with it. Exception: {ex.Message}");
            return localized;
        }
    }
}
