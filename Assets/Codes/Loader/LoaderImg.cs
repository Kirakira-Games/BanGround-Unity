using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderImg : MonoBehaviour
{
    const int ComicCount = 215;
    //private Texture[] loaderSprite;
    private RawImage image;
    private bool play = false;
    private int spriteIndex = 0;

    void Start()
    {
        image = GetComponent<RawImage>();
        int index = Random.Range(1, ComicCount);
        image.texture = Resources.Load<Texture>("Comic/" + index.ToString().PadLeft(5, '0'));
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
