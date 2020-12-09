using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#pragma warning disable 0649
public class SceneTaskDoneEvent : UnityEvent<bool> { }

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject songInfo;
    [SerializeField] private GameObject meme;

    private Animator animator;
    private Camera loaderCamera;
    private AsyncOperation loadOP;

    private static readonly LinkedList<string> sceneStack = new LinkedList<string>();
    private static Func<UniTask<bool>> TaskVoid;
    public static Scene currentScene;
    public static SceneTaskDoneEvent onTaskFinish = new SceneTaskDoneEvent();
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


    private static bool LoadSceneHelper(string nextSceneName, Func<UniTask<bool>> task = null)
    {
        if (Loading) return false;
        // Blocks clicks
        onTaskFinish.RemoveAllListeners();
        MainBlocker.Instance.SetBlock(true);
        Loading = true;

        TaskVoid = task;

        SceneLoader.nextSceneName = nextSceneName;
        SceneManager.LoadScene("Loader", LoadSceneMode.Additive);

        return true;
    }

    private static void PushCurrentScene()
    {
        Debug.Log("Push scene: " + currentScene.name);
        sceneStack.AddLast(currentScene.name);
        if (sceneStack.Count > 10)
        {
            Debug.LogWarning("Too many scenes in stack! Did you forget to use SceneLoader.Back()?\nScene Stack:\n"
                + string.Join("\n", sceneStack.Select(name => "- " + name)));
        }
    }

    private static void PopScene()
    {
        Debug.Log("Pop scene: " + sceneStack.Last.Value);
        sceneStack.RemoveLast();
    }

    public static void LoadScene(string nextSceneName, Func<UniTask<bool>> task = null, bool pushStack = false)
    {
        currentScene = SceneManager.GetActiveScene();
        if (!LoadSceneHelper(nextSceneName, task))
        {
            return;
        }
        if (pushStack)
        {
            onTaskFinish.AddListener(success =>
            {
                if (success)
                {
                    PushCurrentScene();
                }
            });
        }
    }

    public static AsyncOperation LoadSceneAsync(string nextSceneName, bool pushStack = false)
    {
        currentScene = SceneManager.GetActiveScene();
        if (pushStack)
        {
            PushCurrentScene();
        }
        return SceneManager.LoadSceneAsync(nextSceneName);
    }

    /// <summary>
    /// 如果to为null，返回到上一个场景。如果栈为空，则返回场景fallback。
    /// 如果to为栈内场景，则返回至该场景。
    /// </summary>
    public static void Back(string to, string fallback = null)
    {
        currentScene = SceneManager.GetActiveScene();
        if (to == null)
        {
            nextSceneName = sceneStack.Last?.Value ?? fallback;
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError($"No valid scene provided: to={to}, fallback={fallback}.");
                return;
            }
        }
        else if (sceneStack.Find(to) == null)
        {
            Debug.LogError($"Scene {to} is not in the stack.");
            return;
        }
        else
        {
            nextSceneName = to;
        }
        if (!LoadSceneHelper(nextSceneName))
        {
            return;
        }
        onTaskFinish.AddListener(success =>
        {
            if (success)
            {
                if (to == null)
                {
                    if (sceneStack.Count > 0)
                        PopScene();
                }
                else
                {
                    while (sceneStack.Last.Value != to)
                        PopScene();

                    PopScene();
                }
            }
        });
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
            if (TaskVoid != null)
                if (!await TaskVoid()) throw new Exception("Loader task failed.");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            _ = SceneManager.UnloadSceneAsync("Loader");
            Loading = false;
            onTaskFinish.Invoke(false);
            return;
        }
        onTaskFinish.Invoke(true);
        await SceneManager.UnloadSceneAsync(currentScene);
        loadOP = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        loadOP.allowSceneActivation = false;
        while (loadOP.progress < 0.9f)
        {
            await UniTask.DelayFrame(1);
        }
        loadOP.allowSceneActivation = true;

        //开门时间（即loading播放时间） 应减去关门所需时间
        animator.SetTrigger("Open");
        //open gate need 1f
        await UniTask.Delay((int)(seconds * 1000));
        _ = SceneManager.UnloadSceneAsync("Loader");
        Loading = false;
    }

    private void OnDestroy()
    {
        MainBlocker.Instance.SetBlock(false);
    }
}
