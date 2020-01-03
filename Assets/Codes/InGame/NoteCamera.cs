using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCamera : MonoBehaviour
{
    RenderTexture renderTarget;

    void Awake()
    {
        renderTarget = GetComponent<Camera>().targetTexture;
        int size = Screen.height < Screen.width ? Screen.height : Screen.width;

        // Find nearest power of 2
        size--;
        size |= size >> 16;
        size |= size >> 8;
        size |= size >> 4;
        size |= size >> 2;
        size |= size >> 1;
        size++;

        renderTarget.height = size;
        renderTarget.width = size;

        Debug.Log(size);
    }
}
