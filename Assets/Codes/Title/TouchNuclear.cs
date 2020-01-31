using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchNuclear : MonoBehaviour, IPointerClickHandler
{
    private float clickCount = 0;
    private bool egg = false;
    private SpriteRenderer sakana;

    private void Start()
    {
        sakana = GameObject.Find("sakana").GetComponent<SpriteRenderer>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!egg) clickCount += 1;
    }

    private void Update()
    {
        if (!egg)
        {
            if (clickCount >= 10)
            {
                egg = true;
                sakana.sprite = Resources.Load<Sprite>("UI/nuclear");
            }
            else if (clickCount > 0)
            {
                clickCount -= Time.deltaTime * 2;
            }
        }
    }
}
