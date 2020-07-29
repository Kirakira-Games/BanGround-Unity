using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace BGEditor
{
    class MessageBox : MonoBehaviour
    {
        public static MessageBox Instance;
        public Text Title;
        public Text Content;
        public Button Blocker;
        public int result { get; private set; }

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
            //DontDestroyOnLoad(transform.parent.gameObject);
        }

        public static async UniTask<bool> ShowMessage(string title, string content)
        {
            Instance.Show(title, content);
            await UniTask.WaitUntil(() => Instance.result != -1);
            return Instance.result == 1;
        }

        public void Show(string title, string content)
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
}
