using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeOffsetTransform : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(0, LiveSetting.offsetTransform, 0);
    }

}
