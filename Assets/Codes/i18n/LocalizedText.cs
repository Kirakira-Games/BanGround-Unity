using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class LocalizedText : Text
{
    internal static List<LocalizedText> localizedTexts = new List<LocalizedText>();

    public static void ReloadAll()
    {
        for(int i = localizedTexts.Count - 1; i >= 0; i--)
        {
            if(localizedTexts[i] == null)
            {
                localizedTexts.RemoveAt(i);
            }
            else
            {
                localizedTexts[i].Localizify();
            }
        }
    }

    string originalText;
    string cachedText;

    protected override void Start()
    {
        localizedTexts.Add(this);
        originalText = text;

#if UNITY_EDITOR
        Debug.LogWarning("Fuck Editor Start");
        if (UnityEditor.EditorApplication.isPlaying)
        {
            Localizify();
        }
#else
        Localizify();
#endif

        base.Start();
    }

    public void Localizify()
    {
        text = originalText.GetLocalized();
        cachedText = text;
    }

    private void Update()
    {
#if UNITY_EDITOR
        Debug.LogWarning("Fuck Editor Update");
        if (UnityEditor.EditorApplication.isPlaying)
        {
            if (text != cachedText)
            {
                originalText = text;
                Localizify();
            }
        }
#else
        if (text != cachedText)
        {
            originalText = text;
            Localizify();
        }
#endif
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