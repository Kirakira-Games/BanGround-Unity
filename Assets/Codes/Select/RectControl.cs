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

    SelectManager sm;
    public int index;
    Button bt;
    
    // Start is called before the first frame update
    void Start()
    {
        rt_m = GameObject.Find("SongContent").GetComponent<RectTransform>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        sm = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        bt = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        bt.onClick.AddListener(OnPressed);
    }

    void OnPressed()
    {
        sm.SelectSong(index);
    }

    public void OnSelect()
    {
        StartCoroutine(OnSelectAnimation());
    }

    IEnumerator OnSelectAnimation()
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 200);
        yield return new WaitForEndOfFrame();
        float destPos = 0 - rt.anchoredPosition.y - 540;
        while (Math.Abs( rt_m.anchoredPosition.y - destPos) > 1f)
        {
            rt_m.anchoredPosition -= new Vector2(0, (rt_m.anchoredPosition.y - destPos) * 0.3f);
            yield return new WaitForEndOfFrame();
        }
    }

    public void UnSelect()
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 116);
    }

    // Update is called once per frame
    void Update()
    {
        var r = 2096 / 2;
        var position = rt.anchoredPosition.y+(rt_v.sizeDelta.y/2);
        var deltaHeight = Math.Abs(rt_m.anchoredPosition.y+position);
        var angle = Math.Asin(deltaHeight / r);
        float width = (float)(deltaHeight/ Math.Tan(angle)) ;

        if (rt_m.anchoredPosition.y == 0 - rt.anchoredPosition.y - 540)
            width = 1048;

        rt.sizeDelta = new Vector2(width-200f, rt.sizeDelta.y);
    }
}
