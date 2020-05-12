using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

#pragma warning disable 0649
#pragma warning disable 0414
public class TouchEffect : MonoBehaviour
{
    private RectTransform trans;
    private ParticleSystem touchEffect;
    bool isEffectEnabled = false;
    float waiting = 0;
    bool exiting = false;

    private void Start()
    {
        trans = GetComponent<RectTransform>();
        touchEffect = GameObject.Find("ParRoot").GetComponent<ParticleSystem>();
        //SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnSceneChanged(Scene s1, Scene s2)
    {
        isEffectEnabled = s2 == SceneManager.GetSceneByName("InGame") ? false : true;
    }

    // For debugging purpose only, simulate touch event from mouse event
    private static Touch[] SimulateMouseTouch(TouchPhase phase)
    {
        Touch touch = new Touch
        {
            position = Input.mousePosition,
            fingerId = NoteUtility.MOUSE_TOUCH_ID,
            phase = phase
        };
        return new Touch[] { touch };
    }

    private void Update()
    {
        if (isEffectEnabled)
        {
            Touch[] touches = Input.touches;
            if (touches.Length == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touches = SimulateMouseTouch(TouchPhase.Began);
                }
            }
            foreach (var touch in touches)
            {
                trans.anchoredPosition = touch.position;
                touchEffect.Play();
            }
        }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_STANDALONE)
        if (Input.GetKeyUp(KeyCode.Escape) && !exiting)
        {
            waiting += 1.5f;

            if (waiting > 1.5f)
            {
                exiting = true;
                SceneManager.LoadSceneAsync("GameOver", LoadSceneMode.Additive);
            }
            else
            {
                MessageBoxController.ShowMsg(LogLevel.INFO, "Tap Again to End The Game");
            }
        }

        waiting -= Time.deltaTime;
        if (waiting < 0) waiting = 0;
#endif
    }
}
