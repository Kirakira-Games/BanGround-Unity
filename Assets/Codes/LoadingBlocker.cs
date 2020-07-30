using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBlocker : MonoBehaviour
{
    public Text text;
    public Button cancelButton;

    public static LoadingBlocker instance;

    private Action action = null;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OnCancel() => action();
    public void Show(string message, Action cancelAction = null)
    {
        text.text = message;
        gameObject.SetActive(true);

        if (cancelAction == null)
            cancelButton.gameObject.SetActive(false);
        else
            cancelButton.gameObject.SetActive(true);

        action = cancelAction;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
