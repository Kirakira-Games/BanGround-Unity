using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 0649
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject songInfo;
    [SerializeField] private GameObject meme;

    private Animator animator;
    private Camera loaderCamera;
    private AsyncOperation loadOP;

    public static Scene currentSceneName;
    private static string nextSceneName;
    private static bool needOpen;

    public static bool Loading = false;

    void Start()
    {
        if (nextSceneName == "InGame")
        {
            songInfo.SetActive(true);
        }
        else
        {
            meme.SetActive(true);
        }

        animator = GameObject.Find("GateCanvas2").GetComponent<Animator>();
        animator.Play("Closing");
        loaderCamera = GameObject.Find("LoaderCamera").GetComponent<Camera>();
        Load();
    }


    public static void LoadScene(string currentSceneName, string nextSceneName, bool needOpen = false)
    {
        if (Loading) return;
        Loading = true;

        SceneLoader.currentSceneName = SceneManager.GetSceneByName(currentSceneName);
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
        //关门后再加载下一场景（实际关门动画55帧）
        //WaitForSeconds内参数包括了：关门所需时间 + 显示loading小剧场的时间
        yield return new WaitForSeconds(2.3f);
        yield return SceneManager.UnloadSceneAsync(currentSceneName);

        loaderCamera.enabled = true;

        loadOP = SceneManager.LoadSceneAsync(nextSceneName, needOpen ? LoadSceneMode.Additive : LoadSceneMode.Single);
        loadOP.allowSceneActivation = false;

        //开门动画39帧
        //需要开门的话需要等开门动画播放完毕后再卸载Loader场景并重置Loading标志
        if (needOpen)
        {
            StartCoroutine(CountDown(1f));
        }
        else
        {
            loadOP.allowSceneActivation = true;
            Loading = false;
        }
    }

    private IEnumerator CountDown(float seconds)
    {
        while (loadOP.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }
        loadOP.allowSceneActivation = true;

        //开门时间（即loading播放时间） 应减去关门所需时间
        animator.SetTrigger("Open");
        //open gate need 1f
        yield return new WaitForSeconds(seconds);
        SceneManager.UnloadSceneAsync("Loader");
        Loading = false;
    }
}
