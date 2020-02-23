using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FixOrthographicBackground : FixBackground
{
    protected override void Start()
    {
        mainCam = Camera.main;
        float camHeight = mainCam.orthographicSize * 2;
        camSize = new Vector2(mainCam.aspect * camHeight, camHeight);

        render = GetComponent<SpriteRenderer>();
        defaultSprite = render.sprite;

        UpdateScale();
    }

    protected override void UpdateScale()
    {
        Vector2 spriteSize = render.sprite.bounds.size;
        float scale = Mathf.Max(camSize.x / spriteSize.x, camSize.y / spriteSize.y);
        //transform.localScale = new Vector3(camSize.x / spriteSize.x, camSize.y / spriteSize.y, 1);
        transform.localScale = Vector3.one * scale;
    }
}
