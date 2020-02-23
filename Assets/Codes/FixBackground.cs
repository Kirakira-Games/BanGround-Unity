using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FixBackground : MonoBehaviour
{
    protected SpriteRenderer render;
    protected Sprite defaultSprite;
    protected Camera mainCam;
    protected Vector2 camSize;

    float camHeight;
    float camWidth;

    protected virtual void Start()
    {
        /*
            Field of View / 2 == arctan(camheight / (2 * distance))
         */

        mainCam = Camera.main;
        camHeight = Mathf.Tan(mainCam.fieldOfView / 2 * Mathf.Deg2Rad) * 2 * 10.35f;
        camWidth = mainCam.aspect * camHeight;
        //camSize = new Vector2(mainCam.aspect * camHeight, camHeight);

        render = GetComponent<SpriteRenderer>();
        defaultSprite = render.sprite;

        UpdateScale();
    }

    protected virtual void UpdateScale()
    {
        Vector2 spriteSize = render.sprite.bounds.size;
        float scale = Mathf.Max(camWidth / spriteSize.x, camHeight / spriteSize.y);
        //transform.localScale = new Vector3(camSize.x / spriteSize.x, camSize.y / spriteSize.y, 1);
        transform.localScale = Vector3.one * scale;
    }

    public void UpdateBackground(string path)
    {
        if (!File.Exists(path))
        {
            render.sprite = defaultSprite;
            UpdateScale();
            return;
        }
        StartCoroutine(GetAndSetBG(path));
    }

    IEnumerator GetAndSetBG(string path)
    {
        //Debug.Log(path);
//#if UNITY_ANDROID && !UNITY_EDITOR
        path = "file://" + path;
//#endif
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path)) 
        {
            yield return webRequest.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(webRequest);
            render.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        UpdateScale();
    }
}
