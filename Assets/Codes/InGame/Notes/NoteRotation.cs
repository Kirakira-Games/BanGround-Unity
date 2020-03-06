using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRotation : MonoBehaviour
{
    // Start is called before the first frame update
    public bool needRot = true;
    // Update is called once per frame
    void Update()
    {
        if (needRot)
            transform.rotation = Quaternion.Euler(800f / (transform.localPosition.x + 27f) - 54f + Mathf.Pow(2f, 0.024f * transform.localPosition.x), 0f, 0f);
        else
            transform.forward = new Vector3(0,0,1);
    }
}
