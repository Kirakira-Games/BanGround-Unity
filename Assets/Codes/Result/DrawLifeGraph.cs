using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLifeGraph : MonoBehaviour
{
    public Material mat;
    private Texture2D tex;
    private Sprite spr;
    private const int TEX_SIZE = 512;

    void Start()
    {
        tex = new Texture2D(TEX_SIZE, 1);
        if (LifeController.lifePerSecond.Count > 0)
        {
            List<float> lifeList;
            if (LifeController.lifePerSecond.Count < TEX_SIZE)
            {
                lifeList = new List<float>();
                int duplicate = TEX_SIZE / LifeController.lifePerSecond.Count + 1;
                foreach (float i in LifeController.lifePerSecond)
                {
                    for (int j = 0; j < duplicate; j++)
                    {
                        lifeList.Add(i);
                    }
                }
            }
            else
            {
                lifeList = LifeController.lifePerSecond;
            }
            var step = lifeList.Count / TEX_SIZE;
            for (int i = 0; i < TEX_SIZE; i++)
            {
                float sum = 0;
                for (int j = step * i; j < step * (i + 1); j++)
                    sum += lifeList[j];
                sum /= step;
                tex.SetPixel(i, 0, new Color(sum, sum, sum));
            }
        }

        tex.Apply();
        spr = Sprite.Create(tex, new Rect(0, 0, TEX_SIZE, 1), new Vector2(0.5f, 0.5f));

        GetComponent<Image>().overrideSprite = spr;

        //StartCoroutine(DrawLines());
    }

}
