using System;

public interface ILoadingBlocker
{
    void Close();
    void OnCancel();
    void SetText(string message, bool showProgress = false);
    void SetProgress(int current, int max);
    void Show(string message, Action cancelAction = null, bool showProgress = false);
}