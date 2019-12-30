using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleGen : MonoBehaviour
{
    public GameObject circle;
    public float delay;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(gen());
    }

    IEnumerator gen()
    {
        while (true)
        {
            Instantiate(circle, transform.position, transform.rotation, transform);
            yield return new WaitForSeconds(delay);
        }
    }
}
