using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ViewFullBackground : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    List<GameObject> DisableGmobjs = new List<GameObject>();
    List<GameObject> EnableGmobjs = new List<GameObject>();
    private void Start()
    {
        DisableGmobjs.Add(GameObject.Find("Setting_Canvas"));
        DisableGmobjs.Add(GameObject.Find("Select_Canvas"));
        EnableGmobjs.Add(GameObject.Find("FullBaCkGrouNd"));

        foreach (GameObject o in EnableGmobjs) o.SetActive(false);
    }

    public void OnPointerDown(PointerEventData a)
    {
        foreach (GameObject o in DisableGmobjs) o.SetActive(false);
        foreach (GameObject o in EnableGmobjs) o.SetActive(true);
    }

    public void OnPointerUp(PointerEventData a)
    {
        foreach (GameObject o in DisableGmobjs) o.SetActive(true);
        foreach (GameObject o in EnableGmobjs) o.SetActive(false);
    }
}
