using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchEvent : MonoBehaviour
{
    public bool waitingUpdate = true;
    public bool authing = false;

    public UserInfo userCanvas;
    public GameObject warnCanvas;
    public GameObject loginPanel;
    private AsyncOperation operation;
    private bool touched = false;

    private void Start()
    {
        warnCanvas.SetActive(false);
        userCanvas = GameObject.Find("UserInfo").GetComponent<UserInfo>();
    }

    public async void ChangeAnimation()
    {
        if (authing)
            return;

        if (UserInfo.user == null)
        {
            authing = true;
            await GetAuthenticationResult();
            authing = false;
        }

        if (waitingUpdate) 
            return;

        if (!touched)
        {
            touched = true;
            StartSwitch();
        }
    }

    void StartSwitch()
    {
        GameObject.Find("MainCanvas").GetComponent<Animator>().SetBool("Touched", true);
        warnCanvas.SetActive(true);

        StartCoroutine(delayAndSwitch());
        StartCoroutine(TurndownMusic());
    }

    IEnumerator TurndownMusic()
    {
        for (float a = 0.7f; a > 0; a -= 0.1f)
        {
            TitleLoader.instance.music.SetVolume(a);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator delayAndSwitch()
    {
        operation = SceneManager.LoadSceneAsync("Select");
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(1.2f);
        while (operation.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }

        operation.allowSceneActivation = true;
    }

    async UniTask GetAuthenticationResult()
    {
        loginPanel.SetActive(true);
        await UniTask.WaitUntil(() => UserInfo.user != null);
        userCanvas.GetUserInfo().Forget();
    }
}

