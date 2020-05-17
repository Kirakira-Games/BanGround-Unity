using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchEvent : MonoBehaviour
{
    public bool waitingUpdate = true;
    public GameObject warnCanvas;
    public InputField inputField;
    private AsyncOperation operation;
    private bool touched = false;

    private void Start()
    {
        warnCanvas.SetActive(false);
        if (PlayerPrefs.HasKey("key"))
        {
            inputField.SetTextWithoutNotify(PlayerPrefs.GetString("key"));
            inputField.inputType = InputField.InputType.Password;
            inputField.asteriskChar = '☆';
            inputField.readOnly = true;
            inputField.transform.parent.gameObject.SetActive(false);
        }
        else inputField.transform.parent.gameObject.SetActive(true);
    }

    //private IEnumerator SwitchScene(string name)
    //{
    //    yield return new WaitForSeconds(4f);
    //    operation = SceneManager.LoadSceneAsync(name);
    //    operation.allowSceneActivation = false;
    //    yield return operation;
    //}

    public void ChangeAnimation()
    {
#if !UNITY_EDITOR
        if (waitingUpdate) return;
#endif
        if (!touched)
        {
            touched = true;
            StartCoroutine(GetAuthenticationResult());
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

    IEnumerator GetAuthenticationResult()
    {
#if !UNITY_EDITOR
        string uuid = SystemInfo.deviceUniqueIdentifier;
        string key = inputField.text;
        bool usePrefKey = false;
        if (PlayerPrefs.HasKey("key") && inputField.text == PlayerPrefs.GetString("key"))
        {
            usePrefKey = true;
        }
        yield return StartCoroutine(Authenticate.TryAuthenticate(key, uuid));
        if (Authenticate.result.status)
        {
            if (!usePrefKey)
            {
                PlayerPrefs.SetString("key", key);
                PlayerPrefs.Save();
            }
            //MessageBoxController.ShowMsg(LogLevel.OK, "Authenticate successful");
        }
        else
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, Authenticate.result.error);
            if (!Authenticate.isNetworkError || !usePrefKey)
            {
                touched = false;
                inputField.readOnly = false;
                inputField.inputType = InputField.InputType.Standard;
                inputField.transform.parent.gameObject.SetActive(true);
                yield break;
            }
        }
#else
        yield return 0;
#endif

        StartSwitch();
    }
}
