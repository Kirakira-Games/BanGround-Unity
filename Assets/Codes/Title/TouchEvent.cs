using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

    public async void OnStartButtonClick()
    {
        if (authing)
            return;

        //开始游戏前必定触发一次登陆动作
        if (UserInfo.user == null) 
        {
            await GetAuthenticationResult();
            return;
        }

        if (waitingUpdate)
            return;

        if (!touched)
        {
            touched = true;
            StartSwitch();
        }
    }

    public async void OnCommunityButtonClick()
    {
        if (authing)
            return;

        //必须要在线状态才能进社区
        if (UserInfo.user == null || UserInfo.isOffline) 
        {
            await GetAuthenticationResult();
            return;
        }
        SceneLoader.LoadScene("Title", "Community");
    }

    public async void OnLoginButtonClick()
    {
        await GetAuthenticationResult();
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

    private async UniTask GetAuthenticationResult()
    {
        authing = true;

        loginPanel.SetActive(true);
        if (UserInfo.isOffline)
        {
            UserInfo.user = null;
        }

        await UniTask.WaitUntil(() => UserInfo.user != null || !loginPanel.activeSelf);

        if(loginPanel.activeSelf)
        {
            userCanvas.GetUserInfo().Forget();
            loginPanel.SetActive(false);
        }

        authing = false;
    }
}

