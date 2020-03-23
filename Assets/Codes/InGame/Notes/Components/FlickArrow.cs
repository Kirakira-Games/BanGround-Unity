using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickArrow : MonoBehaviour
{
    const float maxY = 1.2f;
    const float speed = 1.8f;
    static Vector3 start = new Vector3(0, 0.6f, 0.5f);

    void Update()
    {
        transform.Translate(transform.up * Time.deltaTime * speed, Space.World);
        if (transform.localPosition.y >= maxY) transform.localPosition = start;
    }
}
