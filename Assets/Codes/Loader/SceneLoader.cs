using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private AsyncOperation operation;
    private Animator animator;

    public static string currentSceneName;
    public static string nextSceneName;
    public static bool needOpen;

    void Start()
    {
        animator = GameObject.Find("GateCanvas").GetComponent<Animator>();
        animator.Play("Closing");
        Load();
    }


    public static void LoadScene(string currentSceneName, string nextSceneName, bool needOpen = false)
    {
        SceneLoader.currentSceneName = currentSceneName;
        SceneLoader.nextSceneName = nextSceneName;
        SceneLoader.needOpen = needOpen;
        SceneManager.LoadScene("Loader", LoadSceneMode.Additive);
    }

    public void Load()
    {
        StartCoroutine(LoadAsync());
        StartCoroutine(UnloadAsync());
        StartCoroutine(CountDown(4f));
    }

    private IEnumerator LoadAsync()
    {
        operation = SceneManager.LoadSceneAsync(nextSceneName,LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        yield return operation;
    }

    private IEnumerator UnloadAsync()
    {
        //close gate time : 2.16f
        yield return new WaitForSeconds(2.3f);
        SceneManager.UnloadSceneAsync(currentSceneName);
    }

    private IEnumerator CountDown(float seconds)
    {
        //True countdown time = seconds - 2.3f
        yield return new WaitForSeconds(seconds);

        operation.allowSceneActivation = true;

        if (needOpen)
        {
            animator.Play("Opening");
            //open gate need 2f
            yield return new WaitForSeconds(2f);
        }
    }
}
