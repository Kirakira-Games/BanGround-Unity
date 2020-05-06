﻿using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class LocalizedStrings : MonoBehaviour
{
    public TextAsset[] languageFiles = new TextAsset[24];
    private Dictionary<string, string> dictionary = null;

    public static LocalizedStrings Instanse = null;

    static KVar cl_language = new KVar("cl_language", "-1", KVarFlags.Archive);

    private void Awake()
    {
        if(cl_language == -1)
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    cl_language.Set((int)Language.SimplifiedChinese);
                    break;
                case SystemLanguage.ChineseTraditional:
                    cl_language.Set((int)Language.TraditionalChinese);
                    break;
                case SystemLanguage.Japanese:
                    cl_language.Set((int)Language.Japanese);
                    break;
                case SystemLanguage.Korean:
                    cl_language.Set((int)Language.Korean);
                    break;
                default:
                    cl_language.Set((int)Language.English);
                    break;
            }
        }

        ReloadLanguageFile(cl_language);

        Instanse = this;
        DontDestroyOnLoad(Instanse.gameObject);
    }

    public string GetLocalizedString(string str)
    {
        if (dictionary.ContainsKey(str))
            return dictionary[str];

        return str;
    }

    public void ReloadLanguageFile(int language)
    {
        dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFiles[(int)language].text);
    }
}