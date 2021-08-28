using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FarClipSliderHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Camera previewCam;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform laneTransform;
    [SerializeField] private GameObject previewObj;
    [SerializeField] private Canvas settingCanvas;

    private Slider farClipSlider;
    private int cullingMask;

    private void Start()
    {
        farClipSlider = GetComponent<Slider>();
        cullingMask = mainCam.cullingMask;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mainCam.cullingMask = 0;
        settingCanvas.worldCamera = previewCam;
        previewObj.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        previewCam.farClipPlane = farClipSlider.value;
        laneTransform.localScale = new Vector3(laneTransform.localScale.x, 1, farClipSlider.value * 0.11838f - 1.00623f);
        laneTransform.position = new Vector3(0f, -0.1f, laneTransform.localScale.z * 5 + 8);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mainCam.cullingMask = cullingMask;
        settingCanvas.worldCamera = mainCam;
        previewObj.SetActive(false);
    }
}
