using System;
using UnityEngine;

public interface ILoadingBlocker
{
    GameObject gameObject { get; }
    void Close();
    void OnCancel();
    void SetText(string message, bool showProgress = false);
    void SetProgress(int current, int max);
    void SetProgress(float progress);
    void SetProgress(ITaskWithProgress task);
    void Show(string message, Action cancelAction = null, bool showProgress = false);
}