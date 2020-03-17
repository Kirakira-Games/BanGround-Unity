using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioProvider;
using System;

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
    [SerializeField] private TextAsset[] startVoices;
    GameObject gateCanvas;

    private ISoundEffect resultVoice;
    private bool isFinished;

    void Start()
    {
        isFinished = false;
        //bg_SR = GameObject.Find("dokidokiBackground").GetComponent<SpriteRenderer>();
        lan_MR = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        
        //var bgColor = LiveSetting.bgBrightness;
        //bg_SR.color = new Color(bgColor, bgColor, bgColor);
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

        PlayVoices();
    }

    private void PlayVoices()
    {
        AudioManager.Instance.PrecacheSE(startVoices[0].bytes).PlayOneShot();
        AudioManager.Instance.PrecacheSE(startVoices[1].bytes).PlayOneShot();
        AudioManager.Instance.PrecacheSE(startVoices[1].bytes).PlayOneShot();
    }

    public void OnPauseButtonClick()
    {
        if (Input.touches.Length >= 2) return;
        StartCoroutine(GamePause());
    }
    public IEnumerator GamePause()
    {
        if (isFinished) yield break;
        while (BitingTheDust)
        {
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 0;
        AudioTimelineSync.instance.Pause();
        AudioManager.Instance.isInGame = false;
        AudioManager.Instance.gameBGM.Pause();
        pause_Canvas.SetActive(true);
    }

    public void GameResume()
    {
        Time.timeScale = 1;
        pause_Canvas.SetActive(false);

        if (AudioManager.Instance.isLoading)
            AudioTimelineSync.instance.Play();
        else
            StartCoroutine(BiteTheDust());
    }

    public static bool BitingTheDust = false;
    private const float BiteTime = 2;

    IEnumerator BiteTheDust()
    {
        ISoundTrack bgm = AudioManager.Instance.gameBGM;
        BitingTheDust = true;
        float currentTime = AudioTimelineSync.instance.GetTimeInS();
        float targetTime = Mathf.Max(0, currentTime - BiteTime);

        // rewind
        bgm.SetPlaybackTime((uint)Mathf.RoundToInt(targetTime * 1000));
        while (currentTime > targetTime)
        {
            currentTime -= Time.deltaTime;
            AudioTimelineSync.instance.Seek(currentTime);
            yield return new WaitForEndOfFrame();
        }

        // play
        uint pauseTime = bgm.GetPlaybackTime();
        bgm.Play();
        while (bgm.GetPlaybackTime() == pauseTime)
        {
            yield return new WaitForEndOfFrame();
        }

        AudioTimelineSync.instance.Seek(bgm.GetPlaybackTime() / 1000f);
        AudioTimelineSync.instance.Play();
        AudioManager.Instance.isInGame = true;
        BitingTheDust = false;
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
        SceneLoader.LoadScene("InGame", "Select", true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (SceneLoader.Loading) return;
        StartCoroutine(GamePause());
    }

    public void OnAudioFinish(bool restart)
    {
        if (SceneLoader.Loading || AudioManager.Instance.isLoading) return;
        isFinished = true;
        AudioManager.Instance.gameBGM?.Dispose();
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
