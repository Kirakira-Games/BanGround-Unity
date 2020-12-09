﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Linq;
using Zenject;
using BanGround;
using UnityEngine.SceneManagement;

#pragma warning disable 0649
#pragma warning disable 0414
public class RectControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private IFileSystem fs;

    RectTransform rt_m;
    RectTransform rt_v;
    RectTransform rt;
    ScrollRect rt_s;
    VerticalLayoutGroup vg;
    DragHandler dh;

    [SerializeField] 
    private Animator deleteAni;

    SelectManager_old sm;
    public int index;
    //Button bt;

    GameObject startImg;
    Text title;
    Image img;

    public Color SelectedColor = Color.white;
    public Color DisabledColor = Color.clear;

    public static Color UnselectColor = new Color(1f, 1f, 1f, 0.8f);

    bool select = false;

    private static readonly string[] delFailMsg = new string[]
    {
        "Please don't be so cruel!",
        "It is a good chart, I promise!",
        "How come? This is your oldest friend in this game.",
        "Nooooooooooooooo",
        "Stop or there will be tons of bugs.",
        "I'll stand by this chart FOREVER!",
        "I won't surrender. Even if you uninstall me.",
        "What's wrong with it? Tell me and I'll try to improve it.",
        "FailedToComeUpWithGoodReasonException: Operation Failed.",
        "Sorry I wasn't paying attention. What was that again?"
    };

    // Start is called before the first frame update
    void Start()
    {
        rt_m = GameObject.Find("SongContent").GetComponent<RectTransform>();
        vg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager_old>();
        startImg = transform.Find("StartImg").gameObject;
        img = GetComponent<Image>();
        title = transform.Find("TextTitle").GetComponent<Text>();

        dh = GameObject.Find("Song Scroll View").GetComponent<DragHandler>();
        //bt = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        //bt.onClick.AddListener(OnPressed);

        rt.sizeDelta = new Vector2(820, 116);
        img.color = DisabledColor;
        title.color = UnselectColor;
        title.fontStyle = FontStyle.Normal;
        startImg.SetActive(false);
        select = false;
    }

    void OnPressed()
    {
        if (SceneLoader.Loading) return;
        if (!select && !SelectManager_old.instance.dh.isDragging)
        {
            StopAllCoroutines();
            sm.SelectSong(index);
        }
        else
        {
            OnEnterPressed();
        }
    }

    public void OnSelect()
    { 
        startImg.SetActive(true);
        select = true;
        img.color = SelectedColor;
        title.color = Color.white;
        title.fontStyle = FontStyle.Bold;
        StartCoroutine(OnSelectAnimation1());
        StartCoroutine(OnSelectAnimation2());
        StartCoroutine(OnSelectAnimation3());
    }

    IEnumerator OnSelectAnimation1()
    {
        yield return new WaitForEndOfFrame();

        //滑动展开
        float destPos = 0 - rt.anchoredPosition.y - vg.padding.top - (rt.sizeDelta.y / 2)+100;
        while (Math.Abs(rt_m.anchoredPosition.y - destPos) > 1f || Math.Abs(rt_s.velocity.y) > 1f)
        {
            rt_m.anchoredPosition -= new Vector2(0, (rt_m.anchoredPosition.y - destPos) * 0.3f);
            yield return new WaitForEndOfFrame();
        }
        rt_m.anchoredPosition = new Vector2(rt_m.anchoredPosition.x, destPos);
    }
    IEnumerator OnSelectAnimation2()
    {
        while (Math.Abs(rt.sizeDelta.y - 190) > 1f)
        {
            rt.sizeDelta -= new Vector2(0, (rt.sizeDelta.y - 190) * 0.3f);
            yield return new WaitForEndOfFrame();
        }
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 190);
    }
    IEnumerator OnSelectAnimation3()
    {
        while (rt.sizeDelta.x < 960)
        {
            rt.sizeDelta += new Vector2((960 - rt.sizeDelta.x) * 0.3f, 0f);
            yield return new WaitForEndOfFrame();
        }
        rt.sizeDelta = new Vector2(960, rt.sizeDelta.y);
    }

    

    public void UnSelect()
    {
        if (!select) return;
        StopAllCoroutines();
        rt.sizeDelta = new Vector2(820, 116);
        img.color = DisabledColor;
        title.color = UnselectColor;
        title.fontStyle = FontStyle.Normal;
        startImg.SetActive(false);
        select = false;
    }

    private void OnEnterPressed()
    {
        if (!dataLoader.MusicExists(chartListManager.current.header.mid))
        {
            messageBannerController.ShowMsg(LogLevel.INFO, "Music missing. Please import it.");
            return;
        }
        //bt.onClick.RemoveAllListeners();
        //bt.interactable = false;
        sm.OnEnterPressed();
    }

    bool down = false;
    float time = 0;
    const float longClickTime = 0.25f;
    const float deleteTime = 2f;
    bool longClick = false;
    bool upProtect = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
        longClick = false;
        time = 0;
        //Debug.Log("Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //if (upProtect) return;
        down = false;
        dh.canDrag = true;
        rt_s.enabled = true;
        deleteAni.Play("DeleteIdle");
        //Debug.Log("Up");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Click");
        if (!longClick) OnPressed();
        else if (time >= longClickTime + deleteTime) OnDelete();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //upProtect = false;
        down = false;
        longClick = false;
        dh.canDrag = true;
        rt_s.enabled = true;
        time = 0;
        deleteAni.Play("DeleteIdle");
        //Debug.Log("exit");
    }


    private void Update()
    {
        if (down)
        {
            time += Time.deltaTime;
            if (!select) return;
            if (time >= longClickTime && !longClick) 
            {
                longClick = true;
                deleteAni.Play("delete");
                dh.canDrag = false;
                rt_s.enabled = false;
                //upProtect = true;
            }
            if (time >= longClickTime + deleteTime)
            {
                //upProtect = true;
            }
        }
    }

    private void OnDelete()
    {
        var header = chartListManager.current.header;
        Difficulty difficulty = chartListManager.current.difficulty;
        if (header.sid == chartListManager.offsetAdjustSid)
        {
            int index = UnityEngine.Random.Range(0, delFailMsg.Length);
            messageBannerController.ShowMsg(LogLevel.INFO, delFailMsg[index]);
            return;
        }

        bool hasOtherDifficulty = false;
        for (int i = 0; i < header.difficultyLevel.Count; i++)
        {
            if (i == (int)difficulty)
                continue;
            if (header.difficultyLevel[i] != -1)
            {
                hasOtherDifficulty = true;
                break;
            }
        }
        if (hasOtherDifficulty)
        {
            dataLoader.DeleteDifficulty(header.sid, difficulty);
        }
        else
        {
            dataLoader.DeleteChart(header.sid);
        }

        SceneLoader.LoadSceneAsync("Select");
    }

    // Update is called once per frame
    /*void Update()
    {
        
        var r = 2096 / 2;
        var position = rt.anchoredPosition.y + (rt_v.sizeDelta.y / 2);
        var deltaHeight = Math.Abs(rt_m.anchoredPosition.y + position);
        var angle = Math.Asin(deltaHeight / r);
        float width = (float)(deltaHeight / Math.Tan(angle));

        width -= 120f;
        
        float width = 0f; 
        if (rt_m.anchoredPosition.y == 0 - rt.anchoredPosition.y - vg.padding.top - 95 || float.IsNaN(width))
            width = 1000;
        else
            width = 890;

        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }*/
}
