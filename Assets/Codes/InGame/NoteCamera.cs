using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCamera : MonoBehaviour
{
    Camera noteCamera;

    void Awake()
    {
        noteCamera = GetComponent<Camera>();
        noteCamera.farClipPlane = LiveSetting.farClip;
    }
}
