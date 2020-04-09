﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour
{
    public static LightControl instance;
    Animator[] anis = new Animator[7];
    MeshRenderer[] rends = new MeshRenderer[7];
    public Color perfectColor;
    public Color greatColor;
    public Color otherColor;
    public Color tapColor;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        for (int i = 0; i < 7; i++)
        {
            var trans = transform.GetChild(i);
            anis[i] = trans.GetComponentInChildren<Animator>();
            rends[i] = trans.GetComponentInChildren<MeshRenderer>();
        }
    }

    public void TriggerLight(int lane, int result = -2)
    {
        if (result == 4||!LiveSetting.laneLight)
            return;
        if (lane < 0 || lane > 7)
            return;
        //print(result);
        switch (result)
        {
            case -2: rends[lane].material.SetColor("_BaseColor",tapColor); break;
            case 0: rends[lane].material.SetColor("_BaseColor", perfectColor); break;
            case 1: rends[lane].material.SetColor("_BaseColor", greatColor); break;
            default: rends[lane].material.SetColor("_BaseColor", otherColor); break;
        }
        anis[lane].SetTrigger("play");
    }
}