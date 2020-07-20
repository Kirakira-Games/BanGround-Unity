﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioProvider;
using System;
using UniRx.Async;

using State = GameStateMachine.State;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
        
        //var bgColor = LiveSetting.bgBrightness;
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
        if (SM.Current == State.Paused || SM.Current == State.Finished || LiveSetting.offsetAdjustMode)
        {
            return;
        }
        InGameBackground.instance.pauseVideo();
        Time.timeScale = 0;
        AudioTimelineSync.instance.Pause();
        AudioManager.Instance.gameBGM.Pause();
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
            _ = BiteTheDust();
        }
    }

    private async UniTaskVoid BiteTheDust()
    {
        ISoundTrack bgm = AudioManager.Instance.gameBGM;
        SM.AddState(State.Rewinding);
        float currentTime = AudioTimelineSync.instance.GetTimeInS();
        float targetTime = Mathf.Max(0, currentTime - BiteTime);

        // rewind
        bgm.SetPlaybackTime((uint)Mathf.RoundToInt(targetTime * 1000));
        while (currentTime > targetTime)
        {
            currentTime -= Time.deltaTime;
            AudioTimelineSync.instance.Seek(currentTime);
            InGameBackground.instance.seekVideo(currentTime);
            InGameBackground.instance.playVideo();
            await UniTask.DelayFrame(0);
            InGameBackground.instance.pauseVideo();
            if (SM.Current != State.Rewinding)
            {
                await UniTask.WaitUntil(() => SM.Current == State.Rewinding);
            }
        }

        // play
        uint pauseTime = bgm.GetPlaybackTime();
        bgm.Play();
        await UniTask.WaitUntil(() => bgm.GetPlaybackTime() != pauseTime);
        AudioTimelineSync.instance.Seek(bgm.GetPlaybackTime() / 1000f);
        AudioTimelineSync.instance.Play();
        InGameBackground.instance.seekVideo(bgm.GetPlaybackTime()/1000f);
        InGameBackground.instance.playVideo();

        if (SM.Current != State.Rewinding)
            await UniTask.WaitUntil(() => SM.Current == State.Rewinding);
        SM.PopState(State.Rewinding);
    }

    public void GameRetry()
    {
        Time.timeScale = 1;

        OnStopPlaying();
        //SceneManager.LoadScene("InGame");
        SceneLoader.LoadScene("InGame", "InGame", true);
    }

    public void GameRetire()
    {
        Time.timeScale = 1;

        OnStopPlaying();
        //SceneManager.LoadScene("Select");
        SceneLoader.LoadScene("InGame", "NewSelect", true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (SceneLoader.Loading) return;
        GamePause();
    }

    public void OnAudioFinish(bool restart)
    {
        if (SceneLoader.Loading || SM.Base == State.Loading) return;
        if (LiveSetting.offsetAdjustMode)
            restart = true;

        InGameBackground.instance.stopVideo();
        AudioManager.Instance.gameBGM?.Dispose();
        AudioManager.Instance.gameBGM = null;
        StartCoroutine(ShowResult(restart));
    }

    IEnumerator DelayDisableGate()
    {
        yield return new WaitForSeconds(3f);
        gateCanvas.SetActive(false);
    }

    IEnumerator ShowResult(bool restart)
    {
        if (LiveSetting.offsetAdjustMode)
            restart = true;

        if (restart)
        {
            SceneManager.LoadSceneAsync("InGame");
        }
        else
        {
            gateCanvas.SetActive(true);
            Image gateImg = GameObject.Find("GateImg").GetComponent<Image>();
            switch (ResultsGetter.GetClearMark())
            {
                case ClearMarks.AP:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/AllPerfect");
                    resultVoice = AudioManager.Instance.PrecacheSE(APvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.FC:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/FullCombo");
                    resultVoice = AudioManager.Instance.PrecacheSE(FCvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.CL:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/Clear");
                    resultVoice = AudioManager.Instance.PrecacheSE(CLvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
                case ClearMarks.F:
                    gateImg.sprite = Resources.Load<Sprite>("UI/ClearMark_Long/Fail");
                    resultVoice = AudioManager.Instance.PrecacheSE(Fvoice.bytes);
                    resultVoice.PlayOneShot();
                    break;
            }
            GameObject.Find("GateCanvas").GetComponent<Animator>().Play("GateClose");
            yield return new WaitForSeconds(3);
            SceneManager.LoadSceneAsync("Result");
        }
    }

    private void OnStopPlaying()
    {
        AudioManager.Instance.gameBGM?.Pause();
        AudioManager.Instance.StopAllCoroutines();

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
        if (SM.Count == 1 && SM.Current != State.Finished &&
            AudioTimelineSync.instance.GetTimeInMs() > AudioManager.Instance.gameBGM.GetLength() + 1000 &&
            NoteController.instance.isFinished)
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
        resultVoice?.Dispose();

/*#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
#endif*/
    }
}