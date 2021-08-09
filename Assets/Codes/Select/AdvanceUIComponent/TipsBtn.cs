using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TipsBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject tipContent;

    private void Awake()
    {
        tipContent = transform.Find("ContentImg").gameObject;
        tipContent.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        tipContent.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        tipContent.SetActive(false);
    }

}
