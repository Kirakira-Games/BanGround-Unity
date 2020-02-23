using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    /*
    void Awake()
    {

        float aspectRatio = (float)Screen.width / Screen.height;
        float size = aspectRatio >= 16f/9 ? 5 : (-3.9375f * aspectRatio + 12f);

        GetComponent<Camera>().orthographicSize = size;
    }
    */

    private void Awake()
    {
        float Ratio = GetComponent<Camera>().aspect;
        float size = Ratio >= 16f/9 ?60f:(-33.3f*Ratio+119.3f );
        // (1,86) (1.78,60) 0.78 -26
        GetComponent<Camera>().fieldOfView = size;
    }
}