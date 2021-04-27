using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class LocalizedText : Text
{
    [Inject]
    LocalizedStrings localizedStrings;

    private static List<LocalizedText> localizedTexts = new List<LocalizedText>();

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
        //Debug.LogWarning("Fuck Editor Start");
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
        if (localizedStrings != null)
        {
            text = localizedStrings.GetLocalizedString(originalText);
        }
        else
        {
            text = originalText;
        }
        cachedText = text;
    }

    public void Localizify(string original)
    {
        originalText = original;
        Localizify();
    }

    private void Update()
    {
#if UNITY_EDITOR
        //Debug.LogWarning("Fuck Editor Update");
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