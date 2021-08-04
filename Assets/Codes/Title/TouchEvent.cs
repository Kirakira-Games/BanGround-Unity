using System.Collections;
using BanGround.Identity;
using UnityEngine;
using Zenject;

public class TouchEvent : MonoBehaviour
{
    [Inject]
    private IAccountManager accountManager;
    [Inject]
    private IMessageBannerController messageBanner;
    [Inject]
    private TitleLoader titleLoader;

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

    public async void OnLoginButtonClick()
    {
        if (await accountManager.DoLogin())
        {
            userCanvas.GetUserInfo().Forget();
        }
    }

    public async void OnStartButtonClick()
    {
        if (waitingUpdate)
            return;

        if (accountManager.isOfflineMode && !accountManager.isTokenSaved && !await accountManager.DoLogin())
        {
            return;
        }

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
        if (accountManager.isOfflineMode && !await accountManager.DoLogin()) 
        {
            return;
        }
        SceneLoader.LoadScene("Community", pushStack: true);
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
            titleLoader.music.SetVolume(a);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator delayAndSwitch()
    {
        operation = SceneLoader.LoadSceneAsync("Select", true);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(1.2f);
        while (operation.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }

        operation.allowSceneActivation = true;
    }
}

