using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderImg : MonoBehaviour
{
    private Texture[] loaderSprite;
    private RawImage image;
    private bool play = false;
    private int spriteIndex = 0;

    void Start()
    {
        loaderSprite = Resources.LoadAll<Texture>("Comic/");
        image = GetComponent<RawImage>();
        image.texture = loaderSprite[Random.Range(0, loaderSprite.Length)];
        image.SetNativeSize();
        //StartCoroutine(DelayPlay(0.5f));
    }

    void Update()
    {
        //if (play)
        //{
        //    spriteIndex += 2;
        //    spriteIndex %= loaderSprite.Length * 2;
        //    image.sprite = loaderSprite[spriteIndex / 2];
        //}
    }

    IEnumerator DelayPlay(float second)
    {
        yield return new WaitForSeconds(second);
        play = true;
    }
}
