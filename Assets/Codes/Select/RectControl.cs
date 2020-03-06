using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class RectControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    RectTransform rt_m;
    RectTransform rt_v;
    RectTransform rt;
    ScrollRect rt_s;
    VerticalLayoutGroup vg;

    SelectManager sm;
    public int index;
    //Button bt;

    GameObject startImg;
    Text title;
    Image img;

    public Color SelectedColor = Color.white;
    public Color DisabledColor = Color.clear;

    bool select = false;

    // Start is called before the first frame update
    void Start()
    {
        rt_m = GameObject.Find("SongContent").GetComponent<RectTransform>();
        vg = GameObject.Find("SongContent").GetComponent<VerticalLayoutGroup>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        rt_s = GameObject.Find("Song Scroll View").GetComponent<ScrollRect>();
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        startImg = transform.Find("StartImg").gameObject;
        img = GetComponent<Image>();
        title = transform.Find("TextTitle").GetComponent<Text>();

        //bt = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        //bt.onClick.AddListener(OnPressed);
        rt.sizeDelta = new Vector2(980, 116);
    }

    void OnPressed()
    {
        if (!select)
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
        StartCoroutine(OnSelectAnimation1());
        StartCoroutine(OnSelectAnimation2());
        StartCoroutine(OnSelectAnimation3());
    }

    IEnumerator OnSelectAnimation1()
    {
        yield return new WaitForEndOfFrame();

        //滑动展开
        float destPos = 0 - rt.anchoredPosition.y - vg.padding.top - (rt.sizeDelta.y / 2);
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
        while (rt.sizeDelta.x < 1050)
        {
            rt.sizeDelta += new Vector2((1050 - rt.sizeDelta.x) * 0.3f, 0f);
            yield return new WaitForEndOfFrame();
        }
        rt.sizeDelta = new Vector2(1050, rt.sizeDelta.y);
    }

    public void UnSelect()
    {
        StopAllCoroutines();
        rt.sizeDelta = new Vector2(980, 116);
        img.color = DisabledColor;
        title.color = Color.grey;
        startImg.SetActive(false);
        select = false;
    }

    bool entering = false;
    private void OnEnterPressed()
    {
        entering = true;
        //bt.onClick.RemoveAllListeners();
        //bt.interactable = false;
        sm.OnEnterPressed();
    }

    float clickTime = 0.8f;
    float boomTime = 2f;
    bool pointerDown = false;
    bool longClick = false;
    Slider progress;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (entering) return;
        progress = GetComponentInChildren<Slider>();
        pointerDown = true;
        longClick = false;
        clickTime = 0.8f;
        boomTime = 2f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        if (longClick)
        {
            progress.value = 0;
        }
        else
        {
            //OnPressed();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!entering && !longClick) OnPressed();
    }


    private void Update()
    {
        if (pointerDown)
        {
            if (clickTime >= 0)
            {
                clickTime -= Time.deltaTime;
            }
            else if (boomTime >= 0) 
            {
                longClick = true;
                boomTime -= Time.deltaTime;
                progress.value = 2 - boomTime;
            }
            else
            {
                OnPointerUp(null);
                OnDelete();
            }
        }
    }

    private void OnDelete()
    {
        var file = DataLoader.GetChartPath(LiveSetting.CurrentHeader.sid, (Difficulty)LiveSetting.actualDifficulty);
        var path = new System.IO.FileInfo(file).Directory.FullName;
        //Debug.Log(path);
        System.IO.Directory.Delete(path, true);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Select");
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
