using Cysharp.Threading.Tasks;

interface IMessageBox
{
    UniTask<bool> ShowMessage(string title, string content);
    UniTask<int> ShowMessage(string title, string content, string[] options);
    bool isActive { get; }
}