using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
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

    private static UniTaskVoid? TaskVoid;
    public static Scene currentSceneName;
    private static string nextSceneName;

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


    public static void LoadScene(string currentSceneName, string nextSceneName, UniTaskVoid? task = null)
    {
        if (Loading) return;
        Loading = true;

        TaskVoid = task;

        SceneLoader.currentSceneName = SceneManager.GetSceneByName(currentSceneName);
        SceneLoader.nextSceneName = nextSceneName;
        SceneManager.LoadScene("Loader", LoadSceneMode.Additive);
        Debug.Log("load");
    }

    public void Load()
    {
        //AudioManagerOld.Instanse.PauseBGM();

        LoadAsync();        
    }

    private async void LoadAsync()
    {
        //关门后再加载下一场景（实际关门动画55帧）
        //时间包括了：关门所需时间 + 显示loading小剧场的时间
        await UniTask.Delay((int)(2.3f*1000));

        loaderCamera.enabled = true;

        //开门动画39帧
        CountDown(.5f);
    }

    private async void CountDown(float seconds)
    {
        try
        {
            if(TaskVoid.HasValue)
             await TaskVoid.Value;
        }
        catch (System.Exception)
        {
            SceneManager.UnloadSceneAsync("Loader");
            Loading = false;
            return;
        }

        await SceneManager.UnloadSceneAsync(currentSceneName);
        loadOP = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        loadOP.allowSceneActivation = false;
        while (loadOP.progress < 0.9f)
        {
            await UniTask.DelayFrame(0);
        }
        loadOP.allowSceneActivation = true;

        //开门时间（即loading播放时间） 应减去关门所需时间
        animator.SetTrigger("Open");
        //open gate need 1f
        await UniTask.Delay((int)(seconds * 1000));
        MainBlocker.Instance.SetBlock(false);
        SceneManager.UnloadSceneAsync("Loader");
        Loading = false;
    }
}
