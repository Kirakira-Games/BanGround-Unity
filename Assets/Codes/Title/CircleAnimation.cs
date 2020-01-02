using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleAnimation : MonoBehaviour
{
    public float translateSpeed;
    public float turnSpeed;
    public float scaleSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0f, 0f, translateSpeed * Time.deltaTime));
        transform.Rotate(new Vector3(0f, 0f, turnSpeed * Time.deltaTime));
        transform.localScale = new Vector3(transform.localScale.x - scaleSpeed * Time.deltaTime, transform.localScale.y - scaleSpeed * Time.deltaTime);
        if (transform.localScale.magnitude < 0.5f) Destroy(gameObject);
    }
}
