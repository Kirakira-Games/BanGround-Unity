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
    
    // Start is called before the first frame update
    void Start()
    {
        rt_m = GameObject.Find("SongContent").GetComponent<RectTransform>();
        rt_v = GameObject.Find("Song Scroll View").GetComponent<RectTransform>();
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        var r = 2096 / 2;
        var position = rt.anchoredPosition.y+(rt_v.sizeDelta.y/2);
        var deltaHeight = Math.Abs(rt_m.anchoredPosition.y+position);
        var angle = Math.Asin(deltaHeight / r);
        float width = (float)(deltaHeight/ Math.Tan(angle)) ;

        rt.sizeDelta = new Vector2(width-200f, rt.sizeDelta.y);
    }
}
