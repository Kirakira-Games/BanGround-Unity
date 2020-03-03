using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneTransform : MonoBehaviour
{
    void Start()
    {
        transform.localScale = new Vector3(transform.localScale.x, 1, LiveSetting.farClip * 0.11838f - 1.00623f);
        transform.position = new Vector3(0f, -0.1f, transform.localScale.z * 5 + 8);
    }

    //8.5,0   169,19 160.5

    void Update()
    {
        
    }
}
