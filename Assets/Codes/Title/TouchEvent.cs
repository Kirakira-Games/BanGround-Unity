using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchEvent : MonoBehaviour
{
    public void SwitchScene(int i)
    {
        SceneManager.LoadSceneAsync(i);
    }
    public void ChangeAnimation()
    {
        GameObject.Find("Main Camera").GetComponent<Animator>().SetBool("Touched", true);
        StartCoroutine(delayAndSwitch());
    }
    IEnumerator delayAndSwitch()
    {
        yield return new WaitForSeconds(2f);
        SwitchScene(1);
    }
}
