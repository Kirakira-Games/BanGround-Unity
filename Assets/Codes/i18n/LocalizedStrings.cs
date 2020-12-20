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

    public TextAsset[] languageFiles = new TextAsset[24];
    private Dictionary<string, string> dictionary = null;

    [Inject(Id = "cl_language")]
    KVar cl_language;

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
    }

    public string GetLocalizedString(string str)
    {
        if (dictionary.ContainsKey(str))
            return dictionary[str];

        Debug.LogWarning($"Missing localized entry: {str}");

        var file = fs.GetOrNewFile("missing_localized_entrys.csv");
        file.WriteBytes(Encoding.UTF8.GetBytes(file.ReadAsString() + "\n" + str));

        return str;
    }

    public void ReloadLanguageFile(int language)
    {
        dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageFiles[(int)language].text);
    }
}