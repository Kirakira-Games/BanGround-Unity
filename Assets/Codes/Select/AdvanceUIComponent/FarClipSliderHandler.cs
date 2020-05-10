using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#pragma warning disable 0649
public class FarClipSliderHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private Slider farClipSlider;
    private Transform oldParent;
    private Vector3 pos;

    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private Canvas mainCanvas;

    [SerializeField] private Transform laneTransform;
    [SerializeField] private Camera previewCam;
    [SerializeField] private Transform newParent;
    [SerializeField] private GameObject previewObj;

    private void Start()
    {
        farClipSlider = GetComponent<Slider>();
        oldParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //var rect = GetComponent<RectTransform>();
        //pos = rect.anchoredPosition;

        previewObj.SetActive(true);
        transform.SetParent(newParent);
        //rect.anchoredPosition = pos;
        settingCanvas.enabled = false;
        mainCanvas.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        previewCam.farClipPlane = farClipSlider.value;
        laneTransform.localScale = new Vector3(laneTransform.localScale.x, 1, farClipSlider.value * 0.11838f - 1.00623f);
        laneTransform.position = new Vector3(0f, -0.1f, laneTransform.localScale.z * 5 + 8);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //var rect = GetComponent<RectTransform>();
        //pos = rect.anchoredPosition;

        transform.SetParent(oldParent);
        //rect.anchoredPosition = pos;
        previewObj.SetActive(false);
        settingCanvas.enabled = true;
        mainCanvas.enabled = true;
    }
}
