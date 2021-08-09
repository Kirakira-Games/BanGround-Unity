using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LaneTransform : MonoBehaviour
{
    float desScaleZ;
    float desPosZ;

    [Inject(Id = "r_farclip")]
    KVar r_farclip;
    void Start()
    {
        desScaleZ = r_farclip * 0.11838f - 1.00623f;
        desPosZ = desScaleZ * 5 + 8;
        StartCoroutine(toSmaller1());
        StartCoroutine(toSmaller2());
    }

    //8.5,0   169,19 160.5
    IEnumerator toSmaller1()
    {
        for (; desPosZ < transform.position.z; transform.position -= new Vector3(0f, 0f, 0.5f))
            yield return new WaitForEndOfFrame();
        transform.position = new Vector3(transform.position.x, transform.position.y, desPosZ);
    }

    IEnumerator toSmaller2()
    {
        for (; desScaleZ < transform.localScale.z; transform.localScale -= new Vector3(0f, 0f, 0.08f))
            yield return new WaitForEndOfFrame();
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, desScaleZ);
    }
}
