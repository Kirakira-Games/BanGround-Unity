﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntInput : MonoBehaviour
{
    public int Default;
    public int MinVal;
    public int MaxVal;
    public int value { get; private set; }

    public Color NormalColor;
    public Color ErrorColor;

    private InputField Component;

    private void Validate(string _)
    {
        if (!int.TryParse(Component.text, out int val) || val <= MinVal || val >= MaxVal)
        {
            Component.textComponent.color = ErrorColor;
        }
        else
        {
            value = val;
            Component.textComponent.color = NormalColor;
        }
    }

    public void SetValue(int value)
    {
        Component.text = value.ToString();
    }

    void Awake()
    {
        Component = GetComponent<InputField>();
        Component.onValueChanged.AddListener(Validate);
        Component.text = Default.ToString();
    }
}