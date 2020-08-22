﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioProvider;
using System;
using UniRx.Async;
using Zenject;

using State = GameStateMachine.State;
using System.Threading;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IAudioTimelineSync audioTimelineSync;
    [Inject]
    private INoteController noteController;

    private const float BiteTime = 2;

    SpriteRenderer bg_SR;
    MeshRenderer lan_MR;

    GameObject pause_Canvas;

    Button pause_Btn;
    Button resume_Btn;
    Button retry_Btn;
    Button retire_Btn;

    public TextAsset APvoice;
    public TextAsset FCvoice;
    public TextAsset CLvoice;
    public TextAsset Fvoice;
    GameObject gateCanvas;

    private ISoundEffect resultVoice;
    public GameStateMachine SM { get; private set; }

    static KVarRef r_brightness_lane = new KVarRef("r_brightness_lane");

    private CancellationTokenSource cancellationToken = new CancellationTokenSource();

    private void Awake()
    {
        Instance = this;
        SM = new GameStateMachine();
        KVarRef lowResolution = new KVarRef("r_lowresolution");
        QualitySettings.SetQualityLevel(lowResolution ? 0 : 1);
    }

    void Start()
    {
        //bg_SR = GameObject.Find("dokidokiBackground").GetComponent<SpriteRenderer>();
        lan_MR = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        
        //var bgColor = liveSetting.bgBrightness;
        //bg_SR.color = new Color(bgColor, bgColor, bgColor);
        lan_MR.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, r_brightness_lane));

        pause_Btn = GameObject.Find("Pause_Btn").GetComponent<Button>();
        resume_Btn = GameObject.Find("Resume_Btn").GetComponent<Button>();
        retry_Btn = GameObject.Find("Retry_Btn").GetComponent<Button>();
        retire_Btn = GameObject.Find("Retire_Btn").GetComponent<Button>();

        pause_Btn.onClick.AddListener(OnPauseButtonClick);
        resume_Btn.onClick.AddListener(GameResume);
        retry_Btn.onClick.AddListener(GameRetry);
        retire_Btn.onClick.AddListener(GameRetire);

        pause_Canvas = GameObject.Find("PauseCanvas");
        pause_Canvas.SetActive(false);

        gateCanvas = GameObject.Find("GateCanvas");
        StartCoroutine(DelayDisableGate());

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif*/

        //Screen.orientation = ScreenOrientation.;
        //MessageBoxController.ShowMsg(LogLevel.INFO, Screen.orientation.ToString());
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeLeft:
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = false;
                break;
            case ScreenOrientation.LandscapeRight:
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = true;
                break;
        }

    }

    public void OnPauseButtonClick()
    {
        if (Input.touches.Length >= 2) return;
        GamePause();
    }

    private void GamePause()
    {
        if (SM.Current == State.Paused || SM.Current == State.Finished || chartListManager.offsetAdjustMode)
        {
            return;
        }
        InGameBackground.instance.pauseVideo();
        Time.timeScale = 0;
        audioTimelineSync.Pause();
        audioManager.gameBGM.Pause();
        pause_Canvas.SetActive(true);
        SM.AddState(State.Paused);

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
#endif*/
    }

    private void GameResume()
    {
        SM.PopState(State.Paused);
        Time.timeScale = 1;
        pause_Canvas.SetActive(false);

        /*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
        #endif*/
        if (SM.Current == State.Playing)
        {
            _ = BiteTheDust(cancellationToken.Token);
        }
        else if (SM.Current == State.Loading)
        {
            audioTimelineSync.Play();
        }
    }

    private async UniTask BiteTheDust(CancellationToken token)
    {
        ISoundTrack bgm = audioManager.gameBGM;
        SM.AddState(State.Rewinding);
        float currentTime = audioTimelineSync.time;
        float targetTime = Mathf.Max(0, currentTime - BiteTime);

        // rewind
        bgm.SetPlaybackTime((uint)Mathf.RoundToInt(targetTime * 1000));
        while (currentTime > targetTime)
        {
            currentTime -= Time.deltaTime;
            audioTimelineSync.time = currentTime;
            InGameBackground.instance.seekVideo(currentTime);
            InGameBackground.instance.playVideo();
            await UniTask.DelayFrame(0);
            InGameBackground.instance.pauseVideo();
            if (SM.Current != State.Rewinding)
            {
                await UniTask.WaitUntil(() => SM.Current == State.Rewinding, cancellationToken: token);
            }
        }

        // play
        uint pauseTime = bgm.GetPlaybackTime();
        bgm.Play();
        await UniTask.WaitUntil(() => bgm.GetPlaybackTime() != pauseTime, cancellationToken: token);
        audioTimelineSync.timeInMs = (int)bgm.GetPlaybackTime();
        audioTimelineSync.Play();
        InGameBackground.instance.seekVideo(bgm.GetPlaybackTime()/1000f);
        InGameBackground.instance.playVideo();

        if (SM.Current != State.Rewinding)
            await UniTask.WaitUntil(() => SM.Current == State.Rewinding, cancellationToken: token);
        SM.PopState(State.Rewinding);
    }

    public void GameRetry()
    {
        //SceneManager.LoadScene("InGame");
        //await liveSetting.LoadChart(true);
        Time.timeScale = 1;
        SceneLoader.LoadScene("InGame", "InGame", () => {
            async UniTask<bool> Retry()
            {
                if (await chartListManager.LoadChart(true))
                {
                    OnStopPlaying();
                    return true;
                }
                Time.timeScale = 0;
                return false;
            }
            return Retry();
        });
    }

    public void GameRetire()
    {
        Time.timeScale = 1;

        OnStopPlaying();
        //SceneManager.LoadScene("Select");
        SceneLoader.LoadScene("InGame", "Select");
    }

    private void OnApplicationPause(bool pause)
    {
        if (SceneLoader.Loading) return;
        GamePause();
    }

    public void OnAudioFinish(bool restart)
    {
        if (SceneLoader.Loading || SM.Base == State.Loading) return;
        if (chartListManager.offsetAdjustMode)
            restart = true;

        InGameBackground.instance.stopVideo();
        audioManager.gameBGM?.Dispose();
        audioManager.gameBGM = null;
        ShowResult(restart);
    }

    IEnumerator DelayDisableGate()
    {
        yield return new WaitForSeconds(3f);
        gateCanvas.SetActive(false);
    }

    async void ShowResult(bool restart)
    {
        if (chartListManager.offsetAdjustMode)
            restart = true;

        if (restart)
        {
            await SceneManager.LoadSceneAsync("InGame");
        }
        else
        {
            gateCanvas.SetActive(true);
            Image gateImg = GameObject.Find("GateImg").GetComponent<Image>();
            switch (ResultsGetter.GetClearMark())
            {
                case ClearMarks.AP:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/AllPerfect");
                    resultVoice = await audioManager.PrecacheSE(APvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.FC:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/FullCombo");
                    resultVoice = await audioManager.PrecacheSE(FCvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.CL:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/Clear");
                    resultVoice = await audioManager.PrecacheSE(CLvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.F:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/Fail");
                    resultVoice = await audioManager.PrecacheSE(Fvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
            }
            GameObject.Find("GateCanvas").GetComponent<Animator>().Play("GateClose");

            await UniTask.Delay(3000);

            _ = SceneManager.LoadSceneAsync("Result");
        }
    }

    private void OnStopPlaying()
    {
        audioManager.gameBGM?.Pause();
        audioManager.StopAllCoroutines();

        pause_Btn.onClick.RemoveAllListeners();
        resume_Btn.onClick.RemoveAllListeners();
        retire_Btn.onClick.RemoveAllListeners();
        retry_Btn.onClick.RemoveAllListeners();

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
#endif*/
    }

    private void Update()
    {
        if (SM.Count == 1 && SM.Current != State.Finished && audioManager.gameBGM != null &&
            audioTimelineSync.timeInMs > audioManager.gameBGM.GetLength() + 1000 &&
            noteController.isFinished)
        {
            SM.Transit(SM.Current, State.Finished);
            OnAudioFinish(false);
        }

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // TODO: Maybe move this
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tilde))
        {
            OnPauseButtonClick();
        }
#endif*/
    }

    private void OnDestroy()
    {
        Instance = null;
        resultVoice?.Dispose();
        cancellationToken.Cancel();

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
#endif*/
    }
}