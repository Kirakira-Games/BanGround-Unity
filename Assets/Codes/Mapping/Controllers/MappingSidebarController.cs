using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MappingSidebarController : MonoBehaviour
{
    public RectTransform target;

    bool state = false;

    public static float EaseInOutQuart(float value)
    {
        value *= 2;

        if (value < 1) 
            return 0.5f * value * value * value * value;

        value -= 2;

        return -1 * 0.5f * (value * value * value * value - 2);
    }

    public async void OnClick()
    {
        var initalPos = target.anchoredPosition;
        var initalRot = transform.localRotation.eulerAngles;

        var targetX = state ? target.sizeDelta.x + 5 : 0f;
        var targetRotZ = state ? 0f : -180.0f;

        state = !state;

        float progress = 0.0f;
        var sub = targetX - initalPos.x;
        var angleSub = targetRotZ - initalRot.z;

        while (progress < 1.0f)
        {
            progress += 0.02f;

            target.anchoredPosition = new Vector2(initalPos.x + sub * EaseInOutQuart(progress), initalPos.y);
            transform.localRotation = Quaternion.Euler(initalRot.x, initalRot.y, initalRot.z + angleSub * EaseInOutQuart(progress));

            await UniTask.DelayFrame(1);
        }

        target.anchoredPosition = new Vector2(targetX, initalPos.y);
        transform.localRotation = Quaternion.Euler(initalRot.x, initalRot.y, targetRotZ);
    }
}
