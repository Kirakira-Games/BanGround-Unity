using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FixOrthographicBackground : FixBackground
{
    Image fullback;
    private void Awake()
    {
        fullback = GameObject.Find("FullBaCkGrouNd").GetComponent<Image>();
    }
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

    protected override void GetAndSetBG(string path)
    {
        var tex = KiraFilesystem.Instance.ReadTexture2D(path);
        render.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        fullback.sprite = render.sprite;
        UpdateScale();
    }
}
