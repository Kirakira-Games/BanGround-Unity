using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class JudgeOffsetTransform : MonoBehaviour
{
    [Inject(Id = "cl_offset_transform")]
    KVar cl_offset_transform;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(0, cl_offset_transform, 0);
    }

}
