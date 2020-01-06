using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    void Awake()
    {

        float aspectRatio = (float)Screen.width / Screen.height;
        float size = aspectRatio >= 16f/9 ? 5 : (-3.9375f * aspectRatio + 12f);

        GetComponent<Camera>().orthographicSize = size;
    }
}