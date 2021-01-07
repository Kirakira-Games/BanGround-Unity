using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentAlphaController : MonoBehaviour
{
    public KiraScrollView scrollView;
    public CanvasGroup canvasGroup;
    public CanvasGroup startCanvasGroup;

    public float fadeSpeed = 0.05f;

    private void Start()
    {
        scrollView.OnMove = () =>
        {
            canvasGroup.alpha = 1;
            startCanvasGroup.alpha = 0;
        };
    }

    void FixedUpdate()
    {
        if(canvasGroup.alpha > 0)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - fadeSpeed);
            startCanvasGroup.alpha = 1 - canvasGroup.alpha;
        }
    }
}
