using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour, IMessageBox
{
    public Text Title;
    public Text Content;
    public Button Blocker;
    private int result;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public async UniTask<bool> ShowMessage(string title, string content)
    {
        Show(title, content);
        await UniTask.WaitUntil(() => result != -1);
        return result == 1;
    }

    public bool isActive => gameObject.activeSelf;

    private void Show(string title, string content)
    {
        if (gameObject.activeSelf)
            throw new InvalidOperationException("Message box is already shown!");
        Blocker.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Title.text = title;
        Content.text = content;
        result = -1;
    }

    public void SetResult(int res)
    {
        result = res;
        Blocker.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}