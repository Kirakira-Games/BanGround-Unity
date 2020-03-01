using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCamera : MonoBehaviour
{
    Camera camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
        camera.farClipPlane = LiveSetting.farClip;
    }
}
