using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RectControl : MonoBehaviour
{
    RectTransform rt_m;
    RectTransform rt_v;
    RectTransform rt;
    ScrollRect rt_s;
    VerticalLayoutGroup vg;

    SelectManager sm;
    public int index;
    Button bt;

    GameObject startImg;

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

        bt = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        bt.onClick.AddListener(OnPressed);
        rt.sizeDelta = new Vector2(900, 116);
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
        StartCoroutine(OnSelectAnimation());
    }

    IEnumerator OnSelectAnimation()
    {
        yield return new WaitForEndOfFrame();
        startImg.SetActive(true);
        select = true;

        float destPos = 0 - rt.anchoredPosition.y - vg.padding.top -(rt.sizeDelta.y/2);
        while (Math.Abs(rt_m.anchoredPosition.y - destPos) > 1f || Math.Abs(rt_s.velocity.y) > 1f)
        {
            rt_m.anchoredPosition -= new Vector2(0, (rt_m.anchoredPosition.y - destPos) * 0.3f);
            yield return new WaitForEndOfFrame();
        }
        rt_m.anchoredPosition = new Vector2(rt_m.anchoredPosition.x, destPos);
        while (Math.Abs(rt.sizeDelta.y - 200) > 1f)
        {
            rt.sizeDelta -= new Vector2(0, (rt.sizeDelta.y - 200) * 0.3f);
            yield return new WaitForEndOfFrame();
        }
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 200);

        while (Math.Abs(rt_m.anchoredPosition.y - destPos) < 1f)
            yield return new WaitForEndOfFrame();
        sm.SelectSong(-1);
        UnSelect();
    }

    public void UnSelect()
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 116);
        startImg.SetActive(false);
        select = false;
    }

    private void OnEnterPressed()
    {
        bt.interactable = false;
        sm.OnEnterPressed();
    }

    // Update is called once per frame
    void Update()
    {
        var r = 2096 / 2;
        var position = rt.anchoredPosition.y + (rt_v.sizeDelta.y / 2);
        var deltaHeight = Math.Abs(rt_m.anchoredPosition.y + position);
        var angle = Math.Asin(deltaHeight / r);
        float width = (float)(deltaHeight / Math.Tan(angle));

        width -= 120f;

        if (rt_m.anchoredPosition.y == 0 - rt.anchoredPosition.y - vg.padding.top - 100 || width <= 460 ||float.IsNaN(width))
            width = 928;

        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }
}
