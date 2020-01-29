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

    public AudioClip APvoice;
    public AudioClip FCvoice;
    public AudioClip CLvoice;
    public AudioClip Fvoice;
    GameObject gateCanvas;


    void Start()
    {
        bg_SR = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        lan_MR = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        
        var bgColor = LiveSetting.bgBrightness;
        bg_SR.color = new Color(bgColor, bgColor, bgColor);
        lan_MR.material.color = new Color(1f, 1f, 1f, LiveSetting.laneBrightness);

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

        am = GameObject.Find("NoteController").GetComponent<AudioManager>();

        gateCanvas = GameObject.Find("GateCanvas");
        StartCoroutine(DelayDisableGate());
    }

    int clickCount = 0;
    public void OnPauseButtonClick()
    {
        clickCount++;
        if (clickCount >= 2)
            GamePause();
    }
    public void GamePause()
    {
        am.PauseBGM();
        pause_Canvas.SetActive(true);
    }

    public void GameResume()
    {
        clickCount = 0;
        am.ResumeBGM();
        pause_Canvas.SetActive(false);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene("InGame");
    }

    public void GameRetire()
    {
        SceneManager.LoadScene("Select");
    }

    private void OnApplicationPause(bool pause)
    {
        GamePause();
    }

    public void OnAudioFinish()
    {
        StartCoroutine(ShowResult());
    }

    IEnumerator DelayDisableGate()
    {
        yield return new WaitForSeconds(3f);
        gateCanvas.SetActive(false);
    }

    IEnumerator ShowResult()
    {
        gateCanvas.SetActive(true);
        Image gateImg = GameObject.Find("GateImg").GetComponent<Image>();
        switch (ResultsGetter.GetClearMark())
        {
            case ClearMarks.AP:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/AllPerfect");
                AudioSource.PlayClipAtPoint(APvoice, Vector3.zero);
                break;
            case ClearMarks.FC:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/FullCombo");
                AudioSource.PlayClipAtPoint(FCvoice, Vector3.zero);
                break;
            case ClearMarks.CL:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/Clear");
                AudioSource.PlayClipAtPoint(CLvoice, Vector3.zero);
                break;
            case ClearMarks.F:
                gateImg.sprite = Resources.Load<Sprite>("UI/SwitchUI/Fail");
                AudioSource.PlayClipAtPoint(Fvoice, Vector3.zero);
                break;
        }
        GameObject.Find("GateCanvas").GetComponent<Animator>().Play("GateClose");
        yield return new WaitForSeconds(3);
        SceneManager.LoadSceneAsync("Result");
    }

}
