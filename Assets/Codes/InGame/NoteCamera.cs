using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NoteCamera : MonoBehaviour
{
    [Inject]
    void Inject([Inject(Id = "r_farclip")]KVar r_farclip)
    {
        var noteCamera = GetComponent<Camera>();
        noteCamera.farClipPlane = r_farclip;
    }
}
