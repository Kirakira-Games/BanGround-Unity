using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    SpriteRenderer bg_SR;
    MeshRenderer lan_MR;
    void Start()
    {
        bg_SR = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        lan_MR = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        var bgColor = LiveSetting.bgBrightness;
        bg_SR.color = new Color(bgColor, bgColor, bgColor);
        lan_MR.material.color = new Color(1f, 1f, 1f, LiveSetting.laneBrightness);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
