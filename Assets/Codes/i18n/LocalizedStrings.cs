using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class LocalizedStrings : MonoBehaviour
{
    public TextAsset[] languageFiles = new TextAsset[24];
    private Dictionary<string, string> dictionary = null;

    public static LocalizedStrings Instanse = null;

    private void Awake()
    {
        ReloadLanguageFile(LiveSetting.language);

        Instanse = this;
        DontDestroyOnLoad(Instanse.gameObject);
    }

    public string GetLocalizedString(string str)
    {
        if (dictionary.ContainsKey(str))
            return dictionary[str];

        return str;
    }

    public void ReloadLanguageFile(Language language)
    {
        if (TitleLoader.IsAprilFool)
            language = Language.Bulgarion;

        dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFiles[(int)language].text);
    }
}