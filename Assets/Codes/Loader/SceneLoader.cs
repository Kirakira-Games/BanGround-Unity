using System;
using System.Collections.Generic;
using System.Linq;
using BanGround.Scene.Params;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#pragma warning disable 0649
public class SceneTaskDoneEvent : UnityEvent<bool> { }

public class SceneState
{
    public dynamic parameters;
    public string name;

    public SceneState() { }

    public SceneState(string name, dynamic parameters)
    {
        this.name = name;
        this.parameters = parameters;
    }

    public SceneState(SceneState rhs)
    {
        name = rhs.name;
        parameters = rhs.parameters;
    }

    public override string ToString()
    {
        return $"{name}, Params = {JsonConvert.SerializeObject(parameters)}";
    }
}

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject songInfo;
    [SerializeField] private GameObject meme;

    private Animator animator;
    private Camera loaderCamera;
    private AsyncOperation loadOP;

    private static readonly LinkedList<SceneState> sceneStack = new LinkedList<SceneState>();
    private static Func<UniTask<bool>> TaskVoid;
    public static SceneState CurrentScene => sceneStack.Last.Value;
    public static dynamic Parameters => CurrentScene.parameters;
    public static T GetParamsOrDefault<T>() where T: new()
    {
        if (Parameters == null)
        {
            Debug.LogWarning($"Missing {nameof(T)}. Falling back to default params.");
            CurrentScene.parameters = new T();
        }
        return Parameters;
    }

    public static SceneTaskDoneEvent onTaskFinish = new SceneTaskDoneEvent();
    private static string nextSceneName;

    public static bool Loading = false;

    public static void Init()
    {
        // Add listener to update current scene
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "Loader" || scene.name == CurrentScene.name)
                return;
            PushScene(scene.name, null);
        };
        
        // Initialize first scene
        PushScene(SceneManager.GetActiveScene().name, null);
    }

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
        LoadAsync();
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

    private static void PushScene(string name, dynamic paramters)
    {
        var state = new SceneState(name, paramters);
        Debug.Log($"Push scene: {state.name}\nParameters: {JsonConvert.SerializeObject(paramters)}");
        sceneStack.AddLast(state);
        if (sceneStack.Count > 10)
        {
            Debug.LogWarning("Too many scenes in stack! Did you forget to use SceneLoader.Back()?\nScene Stack:\n"
                + string.Join("\n", sceneStack.Select(scene => "- " + scene.name)));
        }
    }

    private static void PopScene()
    {
        Debug.Log("Pop scene: " + sceneStack.Last.Value.name);
        sceneStack.RemoveLast();
    }

    public static void LoadScene(string nextSceneName, Func<UniTask<bool>> task = null, bool pushStack = false, dynamic parameters = null)
    {
        if (!LoadSceneHelper(nextSceneName, task))
            return;
        
        onTaskFinish.AddListener(success =>
        {
            if (!success)
                return;
            if (!pushStack)
                PopScene();
            PushScene(nextSceneName, parameters);
        });
    }

    public static AsyncOperation LoadSceneAsync(string nextSceneName, bool pushStack = false, dynamic parameters = null)
    {
        if (!pushStack)
            PopScene();

        PushScene(nextSceneName, parameters);
        
        return SceneManager.LoadSceneAsync(nextSceneName);
    }

    /// <summary>
    /// 如果to为null，返回到上一个场景。如果栈为空，则返回场景fallback。
    /// 如果to为栈内场景，则返回至该场景。
    /// </summary>
    public static void Back(string to, string fallback = null)
    {
        if (to == null)
        {
            nextSceneName = sceneStack.Last.Previous?.Value.name ?? fallback;
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError($"No valid scene provided: to={to}, fallback={fallback}.");
                return;
            }
        }
        else if (sceneStack.All(scene => scene.name != to))
        {
            Debug.LogError($"Scene {to} is not in the stack.");
            return;
        }
        else
        {
            nextSceneName = to;
        }
        if (!LoadSceneHelper(nextSceneName))
            return;

        onTaskFinish.AddListener(success =>
        {
            if (success)
            {
                if (to == null)
                {
                    if (sceneStack.Count > 0)
                    {
                        PopScene();
                    }
                }
                else
                {
                    while (sceneStack.Last.Value.name != to)
                        PopScene();
                }
            }
        });
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
        var currentScene = CurrentScene;
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
        // Must invoke before loading next scene
        onTaskFinish.Invoke(true);
        await SceneManager.UnloadSceneAsync(currentScene.name);
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
