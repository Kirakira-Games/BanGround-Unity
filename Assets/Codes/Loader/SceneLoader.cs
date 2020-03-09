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
        animator = GameObject.Find("GateCanvas2").GetComponent<Animator>();
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
        SceneManager.LoadSceneAsync("Loader", LoadSceneMode.Additive);
    }

    public void Load()
    {
        //AudioManagerOld.Instanse.PauseBGM();

        StartCoroutine(LoadAsync());        
    }

    private IEnumerator LoadAsync()
    {
        //关门后再加载下一场景（实际关门动画40帧）
        //WaitForSeconds内参数包括了：关门所需时间 + 显示loading小剧场的时间
        yield return new WaitForSeconds(2.3f);
        SceneManager.LoadSceneAsync(nextSceneName, needOpen ? LoadSceneMode.Additive : LoadSceneMode.Single);

        //开门动画39帧
        //需要开门的话需要等开门动画播放完毕后再卸载Loader场景并重置Loading标志
        if (needOpen)
        {
            StartCoroutine(CountDown(2f));
        }
        else
        {
            Loading = false;
        }
    }

    private IEnumerator CountDown(float seconds)
    {
        //开门时间（即loading播放时间） 应减去关门所需时间
        SceneManager.UnloadSceneAsync(currentSceneName);
        animator.Play("Opening");
        //open gate need 2f
        yield return new WaitForSeconds(1f);
        SceneManager.UnloadSceneAsync("Loader");
        Loading = false;
    }
}
