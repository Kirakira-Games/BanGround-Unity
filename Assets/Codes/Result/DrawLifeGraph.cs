using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLifeGraph : MonoBehaviour
{
    public Material mat;
    private Texture2D tex;
    private Sprite spr;
    // Start is called before the first frame update
    void Start()
    {
        tex = new Texture2D(256, 1);
        var step = LifeController.lifePerSecond.Count / 256;
        for (int i = 0; i < 256; i++)
        {
            float sum = 0;
            for (int j = step * i; j < step * (i + 1); j++)
                sum += LifeController.lifePerSecond[j];
            sum /= step;
            tex.SetPixel(i, 0, new Color(sum, sum, sum));
        }

        tex.Apply();
        spr = Sprite.Create(tex, new Rect(0, 0, 256, 1), new Vector2(0.5f, 0.5f));

        GetComponent<Image>().overrideSprite = spr;

        //StartCoroutine(DrawLines());
    }

}
