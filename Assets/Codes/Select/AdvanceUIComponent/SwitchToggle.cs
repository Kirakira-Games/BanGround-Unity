using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics.Eventing.Reader;

#pragma warning disable 0649
public class SwitchToggle : MonoBehaviour
{
    private Toggle toggle;
    private RectTransform content;
    private ScrollRect settingRect;
    private RectTransform target;
    private Animator animator;
    [SerializeField] private string panel;
    //[SerializeField] private bool init = false;

    /// <summary>
    /// Returns whether current panel is active.
    /// Result stores the local position of content to scroll to this panel.
    /// </summary>
    public bool GetLocalPosition(out Vector2 result)
    {
        Canvas.ForceUpdateCanvases();

        Vector2 viewportLocalPosition = settingRect.viewport.localPosition;
        Vector2 childLocalPosition = target.localPosition;
        result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return content.localPosition.y >= result.y &&
            content.localPosition.y <= result.y + target.sizeDelta.y - 0.1f;
    }

    private SelectManager sm;

    private void OnToggle(bool active, bool shouldScroll = true)
    {
        if (panel == "Sound_Panel")
        {
            if (active) sm.previewSound?.Pause();
            else sm.previewSound?.Play();
        }
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dropped"))
            return;
        //panelObject.SetActive(active);
        if (active)
        {
            if (shouldScroll && !GetLocalPosition(out var result))
                content.localPosition = result;
        }
    }

    private void Start()
    {
        animator = GameObject.Find("Setting_Canvas").GetComponent<Animator>();
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        content = GameObject.Find("Setting_Panel_Content").GetComponent<RectTransform>();
        settingRect = GameObject.Find("Setting_Panel_Scroll_Rect").GetComponent<ScrollRect>();
        target = GameObject.Find(panel).GetComponent<RectTransform>();
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((active) => OnToggle(active));

        //panelObject.SetActive(init);
    }

    private void Update()
    {
        bool isActive = GetLocalPosition(out var _);
        if (isActive != toggle.isOn)
        {
            toggle.SetIsOnWithoutNotify(isActive);
            OnToggle(isActive, false);
        }
    }
}
