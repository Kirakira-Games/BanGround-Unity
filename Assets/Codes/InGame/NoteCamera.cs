using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCamera : MonoBehaviour
{
    Camera noteCamera;

    static KVarRef r_farclip = new KVarRef("r_farclip");

    void Awake()
    {
        noteCamera = GetComponent<Camera>();
        noteCamera.farClipPlane = r_farclip;
    }
}
