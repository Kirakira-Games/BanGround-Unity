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
            inputField.gameObject.SetActive(false);
        }
    }

    private IEnumerator SwitchScene(string name)
    {
        yield return new WaitForSeconds(4f);
        operation = SceneManager.LoadSceneAsync(name);
        operation.allowSceneActivation = false;
        yield return operation;
    }
    public void ChangeAnimation()
    {
        if (waitingUpdate) return;
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
        StartCoroutine(SwitchScene("Select"));
        StartCoroutine(delayAndSwitch());
    }

    IEnumerator delayAndSwitch()
    {
        yield return new WaitForSeconds(4.5f);
        //SwitchScene("Select");
        //SceneLoader.LoadScene("Title", "Select", true);
        operation.allowSceneActivation = true;
    }

    IEnumerator GetAuthenticationResult()
    {
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
                inputField.gameObject.SetActive(true);
                yield break;
            }
        }
        StartSwitch();
    }
}
