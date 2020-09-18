using System;

public interface ILoadingBlocker
{
    void Close();
    void OnCancel();
    void SetText(string message, bool showProgress = false);
    void Show(string message, Action cancelAction = null, bool showProgress = false);
}