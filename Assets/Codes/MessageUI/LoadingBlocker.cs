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
    private ITaskWithProgress task;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OnCancel() => action();


    public void Show(string message, Action cancelAction = null, bool showProgress = false)
    {
        progress = 0;
        task = null;
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
        task = null;
    }

    public void SetProgress(int current, int max) => SetProgress((float)current / Mathf.Max(1, max));

    public void SetProgress(float progress)
    {
        this.progress = progress;
        showProgress = true;
        UpdateDisplay();
    }

    public void SetProgress(ITaskWithProgress task)
    {
        this.task = task;
        SetProgress(task.Progress);
    }

    private void Update()
    {
        if (task == null || !showProgress)
            return;
        float taskProgress = task.Progress;
        if (NoteUtility.Approximately(progress, taskProgress))
            return;
        SetProgress(taskProgress);
    }
}
