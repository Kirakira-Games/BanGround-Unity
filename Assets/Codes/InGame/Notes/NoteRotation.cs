using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRotation : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(250f/(transform.localPosition.x+20f)-32f + Mathf.Pow(2f,0.03f* transform.localPosition.x), 0f, 0f);
    }
}
