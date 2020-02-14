using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private AsyncOperation operation;
    private Animator animator;

    public static string currentSceneName;
    private static string nextSceneName;
    private static bool needOpen;

    public static bool Loading = false;

    void Start()
    {
        animator = GameObject.Find("GateCanvas").GetComponent<Animator>();
        animator.Play("Closing");
        Load();
    }


    public static void LoadScene(string currentSceneName, string nextSceneName, bool needOpen = false)
    {
        if (Loading) return;
        Loading = true;

        SceneLoader.currentSceneName = currentSceneName;
        SceneLoader.nextSceneName = nextSceneName;
        SceneLoader.needOpen = needOpen;
        SceneManager.LoadScene("Loader", LoadSceneMode.Additive);
    }

    public void Load()
    {
        AudioManager.Instanse.PauseBGM();

        StartCoroutine(LoadAsync());
        //StartCoroutine(UnloadAsync());
        StartCoroutine(CountDown(4f));
    }

    private IEnumerator LoadAsync()
    {
        //关门后再加载下一场景（实际关门时间2.16s）
        yield return new WaitForSeconds(2.3f);
        operation = SceneManager.LoadSceneAsync(nextSceneName, needOpen ? LoadSceneMode.Additive : LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        yield return operation;
    }

    //private IEnumerator UnloadAsync()
    //{
    //    yield return new WaitForSeconds(2.3f);
    //    SceneManager.UnloadSceneAsync(currentSceneName);
    //}

    private IEnumerator CountDown(float seconds)
    {
        //开门时间（即loading播放时间） 应减去关门所需时间
        yield return new WaitForSeconds(seconds);

        if (needOpen) SceneManager.UnloadSceneAsync(currentSceneName);

        operation.allowSceneActivation = true;

        if (needOpen)
        {
            animator.Play("Opening");
            //open gate need 2f
            yield return new WaitForSeconds(2f);
            SceneManager.UnloadSceneAsync("Loader");
        }
        Loading = false;
    }
}
