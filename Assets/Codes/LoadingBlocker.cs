using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBlocker : MonoBehaviour, ILoadingBlocker
{
    public Text text;
    public Button cancelButton;

    private Action action = null;
    private string progress;
    private string message;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OnCancel() => action();

    // TODO: Add progress bar display
    public void Show(string message, Action cancelAction = null, bool showProgress = false)
    {
        progress = "";
        SetText(message, showProgress);

        gameObject.SetActive(true);

        cancelButton.gameObject.SetActive(cancelAction != null);
        action = cancelAction;
    }

    public void SetText(string message, bool showProgress = false)
    {
        this.message = message;
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = message + " " + progress;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(int current, int max)
    {
        progress = $"({current}/{max})";
        UpdateText();
    }
}
