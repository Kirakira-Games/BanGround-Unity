using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class LocalizedText : Text
{
    internal static List<LocalizedText> localizedTexts = new List<LocalizedText>();

    public static void ReloadAll()
    {
        foreach(var text in localizedTexts)
        {
            text.Localizify();
        }
    }

    string originalText;

    protected override void Start()
    {
        localizedTexts.Add(this);
        originalText = text;
        Localizify();

        base.Start();
    }

    public void Localizify()
    {
        text = originalText.GetLocalized();
    }
}

public static class ExtandedMethods
{
    public static string GetLocalized(this string str)
    {
        return LocalizedStrings.Instanse.GetLocalizedString(str);
    }

    public static void Localizify(this string str)
    {
        str = LocalizedStrings.Instanse.GetLocalizedString(str);
    }
}