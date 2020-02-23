using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    SpriteRenderer bg_SR;
    MeshRenderer lan_MR;

    AudioManager am;
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

        am = AudioManager.Instanse;

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
        Time.timeScale = 0;
        am.PauseBGM();
        pause_Canvas.SetActive(true);
    }

    public void GameResume()
    {
        Time.timeScale = 1;
        pause_Canvas.SetActive(false);

        StartCoroutine(BiteTheDust());
    }

    public static bool BitingTheDust = false;

    IEnumerator BiteTheDust()
    {
        am.BGMStream.Position -= LiveSetting.NoteScreenTime;

        BitingTheDust = true;
        int ms = LiveSetting.NoteScreenTime;

        while((ms -= 10) > 0)
        {
            am.lastPos -= 10;
            yield return new WaitForSeconds(0.01f);
        }
        BitingTheDust = false;
        am.ResumeBGM();
    }

    public void GameRetry()
    {
        Time.timeScale = 1;
        AudioManager.Instanse.StopAllCoroutines();

        RemoveListener();
        //SceneManager.LoadScene("InGame");
        SceneLoader.LoadScene("InGame", "InGame");
    }

    public void GameRetire()
    {
        Time.timeScale = 1;
        AudioManager.Instanse.StopAllCoroutines();

        RemoveListener();
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
        if(!SceneLoader.Loading)
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
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/AllPerfect");
                AudioManager.Instanse.StreamSound(APvoice).Play();
                break;
            case ClearMarks.FC:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/FullCombo");
                AudioManager.Instanse.StreamSound(FCvoice).Play();
                break;
            case ClearMarks.CL:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/Clear");
                AudioManager.Instanse.StreamSound(CLvoice).Play();
                break;
            case ClearMarks.F:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/Fail");
                AudioManager.Instanse.StreamSound(Fvoice).Play();
                break;
        }
        GameObject.Find("GateCanvas").GetComponent<Animator>().Play("GateClose");
        yield return new WaitForSeconds(3);
        if (restart) SceneManager.LoadSceneAsync("InGame");
        else SceneManager.LoadSceneAsync("Result");
        //SceneLoader.LoadScene("InGame", "Result", true);
    }

    private void RemoveListener()
    {
        pause_Btn.onClick.RemoveAllListeners();
        resume_Btn.onClick.RemoveAllListeners();
        retire_Btn.onClick.RemoveAllListeners();
        retry_Btn.onClick.RemoveAllListeners();
    }

}
