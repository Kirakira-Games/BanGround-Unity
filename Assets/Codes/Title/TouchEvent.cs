﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchEvent : MonoBehaviour
{
    public bool waitingUpdate = true;
    private AsyncOperation operation;
    private bool touched = false;

    private IEnumerator SwitchScene(string name)
    {
        operation = SceneManager.LoadSceneAsync(name);
        operation.allowSceneActivation = false;
        yield return operation;
    }
    public void ChangeAnimation()
    {
        if (waitingUpdate) return;
        if (!touched)
        {
            touched = true;

            GameObject.Find("MainCanvas").GetComponent<Animator>().SetBool("Touched", true);
            StartCoroutine(SwitchScene("Select"));
            StartCoroutine(delayAndSwitch());
        }
    }
    IEnumerator delayAndSwitch()
    {
        yield return new WaitForSeconds(1f);
        //SwitchScene("Select");
        //SceneLoader.LoadScene("Title", "Select", true);
        operation.allowSceneActivation = true;
    }
}
