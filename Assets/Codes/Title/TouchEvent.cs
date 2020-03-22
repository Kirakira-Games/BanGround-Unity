using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchEvent : MonoBehaviour
{
    public bool waitingUpdate = true;
    public Text keyInput;
    private InputField inputField;
    private AsyncOperation operation;
    private bool touched = false;

    private void Start()
    {
        inputField = keyInput.GetComponentInParent<InputField>();
        if (PlayerPrefs.HasKey("key"))
        {
            inputField.SetTextWithoutNotify(PlayerPrefs.GetString("key"));
            keyInput.transform.parent.gameObject.SetActive(false);
        }
    }

    private IEnumerator SwitchScene(string name)
    {
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
        StartCoroutine(SwitchScene("Select"));
        StartCoroutine(delayAndSwitch());
    }

    IEnumerator delayAndSwitch()
    {
        yield return new WaitForSeconds(1f);
        //SwitchScene("Select");
        //SceneLoader.LoadScene("Title", "Select", true);
        operation.allowSceneActivation = true;
    }

    IEnumerator GetAuthenticationResult()
    {
        string uuid = SystemInfo.deviceUniqueIdentifier;
        string key = keyInput.text;
#if UNITY_EDITOR
        uuid = "1145141919810";
#endif
        bool usePrefKey = false;
        if (PlayerPrefs.HasKey("key") && keyInput.text == PlayerPrefs.GetString("key"))
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
            MessageBoxController.ShowMsg(LogLevel.OK, "Authenticate successful");
        }
        else
        {
            MessageBoxController.ShowMsg(LogLevel.ERROR, Authenticate.result.error);
            if (!Authenticate.isNetworkError || !usePrefKey)
            {
                touched = false;
                yield break;
            }
        }
        StartSwitch();
    }
}
