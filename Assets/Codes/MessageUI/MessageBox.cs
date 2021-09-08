using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour, IMessageBox
{
    public Text Title;
    public Text Content;
    public Button Blocker;
    public GameObject Buttons;
    public GameObject ButtonPrefab;

    private Button[] mYesNoButtons;
    private Button[] mCustomButtons;
    private int result;

    private void Awake()
    {
        mYesNoButtons = Buttons.transform.GetComponentsInChildren<Button>();
        gameObject.SetActive(false);
    }

    private void DestroyButtons()
    {
        foreach (var button in mCustomButtons)
        {
            Destroy(button.gameObject);
        }
        mCustomButtons = null;
    }

    public async UniTask<bool> ShowMessage(string title, string content)
    {
        Show(title, content);
        await UniTask.WaitUntil(() => result != -1);
        return result == 1;
    }

    public async UniTask<int> ShowMessage(string title, string content, params string[] options)
    {
        Show(title, content, options);
        await UniTask.WaitUntil(() => result != -1);
        DestroyButtons();
        return result;
    }

    public bool isActive => gameObject.activeSelf;

    private void Show(string title, string content, string[] options = null)
    {
        if (gameObject.activeSelf)
            throw new InvalidOperationException("Message box is already shown!");
        Blocker.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Title.text = title;
        Content.text = content;
        result = -1;

        // Use yes/no buttons or options
        foreach (var button in mYesNoButtons)
        {
            button.gameObject.SetActive(options == null);
        }
        if (options != null)
        {
            mCustomButtons = new Button[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var button = Instantiate(ButtonPrefab, Buttons.transform).GetComponent<Button>();
                int resultId = i;
                button.GetComponentInChildren<Text>().text = options[i];
                button.onClick.AddListener(() => SetResult(resultId));
                mCustomButtons[i] = button;
            }
        }
    }

    public void SetResult(int res)
    {
        result = res;
        Blocker.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
