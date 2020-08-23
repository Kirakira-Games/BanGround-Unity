using UniRx.Async;

interface IMessageBox
{
    UniTask<bool> ShowMessage(string title, string content);
    bool isActive { get; }
}