using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchEvent : MonoBehaviour
{
    public bool waitingUpdate = true;
    public bool waitingAuth => Authenticate.user == null;
    public bool authing => Authenticate.isAuthing;

    public UserInfo usercanvas;
    public GameObject warnCanvas;
    public GameObject loginPanel;
    private AsyncOperation operation;
    private bool touched = false;

    private void Start()
    {
        warnCanvas.SetActive(false);
        usercanvas = GameObject.Find("UserInfo").GetComponent<UserInfo>();
    }

    //private IEnumerator SwitchScene(string name)
    //{
    //    yield return new WaitForSeconds(4f);
    //    operation = SceneManager.LoadSceneAsync(name);
    //    operation.allowSceneActivation = false;
    //    yield return operation;
    //}

    public async void ChangeAnimation()
    {
//#if !UNITY_EDITOR
        if (!authing) 
            await GetAuthenticationResult();
        else
            return;

        if (waitingUpdate || waitingAuth) 
            return;
//#endif
        if (!touched)
        {
            touched = true;
            StartSwitch();
            //StartCoroutine(GetAuthenticationResult());
        }
    }

    void StartSwitch()
    {
        GameObject.Find("MainCanvas").GetComponent<Animator>().SetBool("Touched", true);
        warnCanvas.SetActive(true);

        //StartCoroutine(SwitchScene("Select"));
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

        //SwitchScene("Select");
        //SceneLoader.LoadScene("Title", "Select", true);
        operation.allowSceneActivation = true;
    }

    async UniTask GetAuthenticationResult()
    {
        if(Authenticate.user == null)
        {
            if(!Authenticate.isAuthing)
                loginPanel.SetActive(true);

            await UniTask.WaitUntil(() => Authenticate.user != null);
        }

        usercanvas.GetUserInfo();
        /*
//#if !UNITY_EDITOR
        authing = true;
        string uuid = AppPreLoader.UUID;
        string key = inputField.text;
        bool usePrefKey = false;
        if (PlayerPrefs.HasKey("key") && inputField.text == PlayerPrefs.GetString("key"))
        {
            usePrefKey = true;
        }
        var result = await Authenticate.TryAuthenticate(key, uuid);
        if (result.status)
        {
            if (!usePrefKey)
            {
                PlayerPrefs.SetString("key", key);
                PlayerPrefs.Save();
            }
            waitingAuth = false;
            UserInfo.username = result.username;
            usercanvas.GetUserInfo();
            //Debug.Log(Authenticate.result.username);
            //MessageBoxController.ShowMsg(LogLevel.OK, "Authenticate successful");
        }
        else
        {
            messageBannerController.ShowMsg(LogLevel.ERROR, result.error);
            if (!Authenticate.isNetworkError || !usePrefKey)
            {
                //touched = false;
                inputField.readOnly = false;
                inputField.inputType = InputField.InputType.Standard;
                inputField.transform.parent.gameObject.SetActive(true);
                authing = false;
                return;
            }
            else
            {
                waitingAuth = false;
            }
        }
        authing = false;
//#else
//        yield return 0;
//#endif
        */
    }
}

