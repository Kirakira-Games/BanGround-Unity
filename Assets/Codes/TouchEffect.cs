using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchEffect : MonoBehaviour
{
    private RectTransform trans;
    private ParticleSystem touchEffect;

    private void Start()
    {
        trans = GetComponent<RectTransform>();
        touchEffect = GameObject.Find("ParRoot").GetComponent<ParticleSystem>();
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
}
