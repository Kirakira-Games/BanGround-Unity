using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

[Serializable]
public class TabToggleGroupPanel
{
    public Toggle Toggle;
    public RectTransform Target;
}

public class TabToggleGroup : MonoBehaviour
{
    public RectTransform Content;
    public ScrollRect Scroll;
    public TabToggleGroupPanel[] Tabs;

    public int ActiveTabIndex { get; private set; }
    public TabToggleGroupPanel ActiveTab => Tabs[ActiveTabIndex];

    /// <summary>
    /// Returns the top position of a panel. Must call <see cref="Canvas.ForceUpdateCanvases()"/> before calling this function.
    /// </summary>
    private Vector2 GetTopPosition(RectTransform panel)
    {
        Vector2 childLocalPosition = panel.localPosition;
        return new Vector2(
            0,
            0 - childLocalPosition.y
        );
    }

    private int GetActiveTabIndex()
    {
        Canvas.ForceUpdateCanvases();

        for (var i = 0; i < Tabs.Length; i++)
        {
            var tab = Tabs[i];
            var pos = GetTopPosition(tab.Target);
            if (Content.localPosition.y <= pos.y + tab.Target.sizeDelta.y - 0.1f)
                return i;
        }

        return 0;
    }

    private void OnToggle(int index, bool isOn)
    {
        if (isOn)
        {
            if (index != ActiveTabIndex)
            {
                ActiveTabIndex = index;
                Canvas.ForceUpdateCanvases();
                Content.localPosition = GetTopPosition(ActiveTab.Target);
            }
        }
    }

    void Start()
    {
        ActiveTabIndex = GetActiveTabIndex();
        for (int i = 0; i < Tabs.Length; i++)
        {
            int index = i;
            Tabs[i].Toggle.onValueChanged.AddListener(isOn => OnToggle(index, isOn));
        }
    }

    void Update()
    {
        int index = GetActiveTabIndex();
        if (index != ActiveTabIndex)
        {
            ActiveTabIndex = index;
            ActiveTab.Toggle.isOn = true;
        }
    }
}
