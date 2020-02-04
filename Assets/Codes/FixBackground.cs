using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FixBackground : MonoBehaviour
{
    SpriteRenderer render;
    Sprite defaultSprite;

    private void Start()
    {
        render = GetComponent<SpriteRenderer>();
        defaultSprite = render.sprite;
        UpdateScale();
    }

    private void UpdateScale()
    {
        int width = render.sprite.texture.width;
        int height = render.sprite.texture.height;
        transform.localScale = new Vector3(Screen.width / (float)width, Screen.height / (float)height, 1);
    }

    public void UpdateBackground(string path)
    {
        if (path == null)
        {
            render.sprite = defaultSprite;
            UpdateScale();
            return;
        }
        StartCoroutine(GetAndSetBG(path));
    }

    IEnumerator GetAndSetBG(string path)
    {
        Debug.Log(path);
#if UNITY_ANDROID && !UNITY_EDITOR
        path = "file://" + path;
#endif
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path)) 
        {
            yield return webRequest.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(webRequest);
            render.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        UpdateScale();
    }
}
