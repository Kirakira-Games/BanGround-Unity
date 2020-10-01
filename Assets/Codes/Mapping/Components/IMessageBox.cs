using Cysharp.Threading.Tasks;

interface IMessageBox
{
    UniTask<bool> ShowMessage(string title, string content);
    bool isActive { get; }
}