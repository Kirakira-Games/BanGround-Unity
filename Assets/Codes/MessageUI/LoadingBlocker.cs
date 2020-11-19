using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBlocker : MonoBehaviour, ILoadingBlocker
{
    public Text text;
    public Button cancelButton;
    public Slider progressBar;
    public GameObject loadingIcon;
    public float progress { get; private set; }
    public bool showProgress { get; private set; }

    private Action action = null;
    private string message;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OnCancel() => action();

    // TODO: Add progress bar display
    public void Show(string message, Action cancelAction = null, bool showProgress = false)
    {
        progress = 0;
        SetText(message, showProgress);

        gameObject.SetActive(true);

        cancelButton.gameObject.SetActive(cancelAction != null);
        action = cancelAction;
    }

    public void SetText(string message, bool showProgress = false)
    {
        this.message = message;
        this.showProgress = showProgress;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        text.text = message;
        if (showProgress == loadingIcon.activeSelf)
        {
            loadingIcon.SetActive(!showProgress);
            progressBar.gameObject.SetActive(showProgress);
        }
        if (showProgress)
        {
            progressBar.value = progress;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(int current, int max) => SetProgress((float)current / Mathf.Max(1, max));

    public void SetProgress(float progress)
    {
        this.progress = progress;
        UpdateDisplay();
    }
}
