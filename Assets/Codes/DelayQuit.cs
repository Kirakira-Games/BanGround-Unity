using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayQuit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    IEnumerator DelayQ()
    {
        yield return new WaitForSeconds(6f);
        Application.Quit();
    }
}
