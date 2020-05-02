using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeOffsetTransform : MonoBehaviour
{
    static KVarRef cl_offset_transform = new KVarRef("cl_offset_transform");

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(0, cl_offset_transform, 0);
    }

}