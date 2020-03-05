﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioProvider;

public class UIManager : MonoBehaviour
{
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

    void Start()
    {
        bg_SR = GameObject.Find("dokidokiBackground").GetComponent<SpriteRenderer>();
        lan_MR = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        
        var bgColor = LiveSetting.bgBrightness;
        bg_SR.color = new Color(bgColor, bgColor, bgColor);
        lan_MR.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, LiveSetting.laneBrightness));

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
    }

    public void OnPauseButtonClick()
    {
        if (Input.touches.Length >= 2) return;
        GamePause();
    }
    public void GamePause()
    {
        AudioManager.Instance.isInGame = false;
        Time.timeScale = 0;
        AudioManager.Instance.gameBGM.Pause();
        pause_Canvas.SetActive(true);
    }

    public void GameResume()
    {
        Time.timeScale = 1;
        pause_Canvas.SetActive(false);

        //StartCoroutine(BiteTheDust());
        AudioManager.Instance.gameBGM.Play();
        AudioManager.Instance.isInGame = true;
    }

    public static bool BitingTheDust = false;
    public static uint biteTime = 0;

    IEnumerator BiteTheDust()
    {
        ISoundTrack bgm = AudioManager.Instance.gameBGM;

        BitingTheDust = true;
        uint pos = bgm.GetPlaybackTime();
        if (pos < LiveSetting.NoteScreenTime) pos = 0;
        else pos -= (uint)LiveSetting.NoteScreenTime;
        bgm.SetPlaybackTime(pos);

        int ms = LiveSetting.NoteScreenTime;

        while((ms -= 10) > 0)
        {
            pos -= 10;
            biteTime = pos;
            yield return new WaitForSeconds(0.01f);
        }
        BitingTheDust = false;
        bgm.Play();
        AudioManager.Instance.isInGame = true;
    }

    public void GameRetry()
    {
        Time.timeScale = 1;

        OnStopPlaying();
        //SceneManager.LoadScene("InGame");
        SceneLoader.LoadScene("InGame", "InGame",true);
    }

    public void GameRetire()
    {
        Time.timeScale = 1;

        OnStopPlaying();
        //SceneManager.LoadScene("Select");
        SceneLoader.LoadScene("InGame", "Select", true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (SceneLoader.Loading) return;
        GamePause();
    }

    public void OnAudioFinish(bool restart)
    {
        if (SceneLoader.Loading) return;
        StartCoroutine(ShowResult(restart));
    }

    IEnumerator DelayDisableGate()
    {
        yield return new WaitForSeconds(3f);
        gateCanvas.SetActive(false);
    }

    IEnumerator ShowResult(bool restart)
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
        if (restart) SceneManager.LoadSceneAsync("InGame");
        else SceneManager.LoadSceneAsync("Result");
        //SceneLoader.LoadScene("InGame", "Result", true);
    }

    private void OnStopPlaying()
    {
        AudioManager.Instance.gameBGM?.Pause();
        AudioManager.Instance.StopAllCoroutines();

        pause_Btn.onClick.RemoveAllListeners();
        resume_Btn.onClick.RemoveAllListeners();
        retire_Btn.onClick.RemoveAllListeners();
        retry_Btn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (AudioManager.Instance.isInGame && AudioManager.Instance.gameBGM.GetStatus() != PlaybackStatus.Playing)
        {
            AudioManager.Instance.isInGame = false;
            OnAudioFinish(false);
        }
    }

    private void OnDestroy()
    {
        resultVoice?.Dispose();
    }
}
