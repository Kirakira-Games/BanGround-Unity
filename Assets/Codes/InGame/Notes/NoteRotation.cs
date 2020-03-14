using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRotation : MonoBehaviour
{
    public bool needRot = true;
    private float mainCamY;
    private float mainCamZ;

    private void Start()
    {
        mainCamY = Camera.main.transform.position.y;
        mainCamZ = Camera.main.transform.position.z;
    }

    void Update()
    {
        if (needRot)
        {
            //transform.rotation = Quaternion.Euler(800f / (transform.localPosition.x + 27f) - 54f + Mathf.Pow(2f, 0.024f * transform.localPosition.x), 0f, 0f);
            Vector3 dir = new Vector3(0, mainCamY, mainCamZ - transform.position.z);//new Vector3(transform.position.x, mainCamY, mainCamZ) - transform.position;
            float abs = Mathf.Abs(dir.z);
            Debug.Log(abs);
            if (abs > 30) abs = 8;
            else if (abs < 15) abs = 30;
            else abs = 150 / (2 * abs - 24.2f) + 4;
            dir.z = abs;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
            transform.forward = new Vector3(0, 0, 1);
    }
}
